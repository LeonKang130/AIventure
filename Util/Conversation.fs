﻿module Util.Conversation
open System
open Communication.ThroughBackend
open Godot
open Characters

type ConversationState =
    | PlayerSpeaking
    | NPCSpeaking
    | NPCWaiting
    | Inactive

type ConversationManager(dialog: CanvasLayer, directory: string) =
    let mutable name: string option = None
    let mutable prioritized: (string * string) list = []
    let mutable current = ConversationState.Inactive
    let mutable streamed = false;
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
    member this.OnContinueDialog() =
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
                        chatBotHandler.GetAPIResponse character query
                            |> (fun task -> task.WaitAsync timeout)
                            |> Async.AwaitTask
                            |> Async.Catch
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
        | _ -> ()