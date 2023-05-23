module Util.Conversation
open System
open Communication.ThroughBackend
open Godot
open Characters

type ConversationState =
    | PlayerSpeaking
    | NPCSpeaking
    | NPCWaiting
    | Inactive

type ConversationManager(dialog: CanvasLayer) =
    let directory =
        if OS.HasFeature("editor") then
            ProjectSettings.GlobalizePath("characters")
        else
            OS.GetExecutablePath().GetBaseDir().PathJoin("characters")
    let mutable name: string option = None
    let mutable prioritized: (string * string) list = []
    let mutable current = ConversationState.Inactive
    let mutable streamed = false
    let mutable requestExit = false
    let mutable requestExitPending = false
    let portrait = dialog.GetNode<TextureRect>("HSplitContainer/PortraitMargin/TextureRect")
    let label = dialog.GetNode<Label>("HSplitContainer/TextMargin/ScrollContainer/VBoxContainer/Label")
    let textEdit = dialog.GetNode<TextEdit>("HSplitContainer/TextMargin/TextEdit")
    let chatBotHandler = ChatBotHandler(directory)
    let characterManager = CharacterManager(directory)
    member this.InConversation() =
        dialog.Visible
    member this.ShowMessage (character: string) (message: string) =
        current <- NPCSpeaking
        dialog.Show()
        portrait.Texture <- characterManager.CharacterPortraits[character.ToLower()]
        label.Text <- $"{character}: {message}"
        label.Show()
        textEdit.Hide()
    member this.ShowTextEdit() =
        current <- PlayerSpeaking
        dialog.Show()
        portrait.Texture <- characterManager.CharacterPortraits["slime"]
        label.Hide()
        textEdit.Clear()
        textEdit.Show()
    member this.HideDialog() =
        current <- Inactive
        dialog.Hide()
    member this.GetInteractableCharacterName() = name.Value
    member this.SetInteractableCharacterName character =
        name <-
            if character = null then
                None
            else
                Some(character)
    member this.EnqueuePrioritizedDialog (name: string) (message: string) =
        prioritized <- prioritized @ [(name, message)]
    member this.OnContinueDialog(groundTruth: string) =
        match current with
        | Inactive | NPCWaiting -> ()
        | NPCSpeaking ->
            if prioritized.Length <> 0 then
                let (character, message) :: successors = prioritized
                prioritized <- successors
                streamed <- true
                this.ShowMessage character message
            else if streamed then
                streamed <- false
                this.HideDialog()
            else
                this.ShowTextEdit()
        | PlayerSpeaking ->
            let query = textEdit.Text.Trim()
            match name, query with
            | _, "" | None, _ -> this.HideDialog()
            | Some(character), query ->
                this.ShowMessage character "..."
                current <- NPCWaiting
                async {
                    let timeout =
                        TimeSpan(0, 0, 5)
                    let! result =
                        chatBotHandler.GetAPIResponse character query groundTruth
                            |> (fun task -> task.WaitAsync timeout)
                            |> Async.AwaitTask
                            |> Async.Catch
                    GD.Print (sprintf "%A" result)
                    match result with
                    | Choice1Of2 response ->
                        this.ShowMessage character response
                    | Choice2Of2 _ ->
                        this.ShowMessage character "(unrecognized mumbling...)"
                }
                |> Async.Start
    member this.OnQuitDialog() =
        match current with
        | Inactive | NPCWaiting | PlayerSpeaking -> ()
        | NPCSpeaking ->
            streamed <- false
            prioritized <- []
            this.HideDialog()
    member this.PlayPrioritizedDialog() =
        match current with
        | Inactive ->
            if prioritized.Length <> 0 then
                let (character, message) :: successors = prioritized
                prioritized <- successors
                streamed <- true
                this.ShowMessage character message
            else
                requestExit <- requestExitPending
        | _ -> ()
    member this.IsEmpty() =
        prioritized.IsEmpty
    member this.RequestExit() =
        requestExit
    member this.OnTrapEntered() =
        this.EnqueuePrioritizedDialog "Slime" "Damn, I think I just stepped into a trap!"
        this.EnqueuePrioritizedDialog "Slime" "I guess I can't make it to the presentation now, hehe."
        this.EnqueuePrioritizedDialog "Slime" "(Dead...)"
        requestExitPending <- true
    member this.OnTrapAvoided (name: string) =
        this.EnqueuePrioritizedDialog "Slime" "Damn, I think I just stepped into a trap!"
        this.EnqueuePrioritizedDialog "Slime" "Wait, I'm not hurt?!"
        this.EnqueuePrioritizedDialog "Slime" $"The {name} saved me! But it seems to be broken now. I'll just leave it here."
    member this.OnDestinationReached() =
        this.EnqueuePrioritizedDialog "Devil" "Are you one of the new employees? You're late."
        this.EnqueuePrioritizedDialog "Slime" "I'm so sorry. There are too many traps around here."
        this.EnqueuePrioritizedDialog "Devil" "Never mind. Few employees can survive the traps in the labyrinth. That's why we hire so often. One thing I love about these traps is that few can make it to ask for medical insurance alive. You know..."
        this.EnqueuePrioritizedDialog "Devil" "(Click...)"
        this.EnqueuePrioritizedDialog "Devil" "Wait, was that a ..."
        this.EnqueuePrioritizedDialog "Slime" "I am afraid you triggered the trap, boss."
        this.EnqueuePrioritizedDialog "Devil" "(Dead...)"
        this.EnqueuePrioritizedDialog "Slime" "Well, the presentation is cancelled now, I guess. These slides took me two days to finish, damn it."
        requestExitPending <- true
    member this.CharacterList =
        characterManager.CharacterList 