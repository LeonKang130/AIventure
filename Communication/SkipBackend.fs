module Communication.SkipBackend
open System
open System.Collections.Generic
open System.IO
open FSharp.Core
open Godot
open Microsoft.FSharp.Collections
open OpenAI
open OpenAI.Chat

type ChatBotHandler(directory: string) =
    do GD.Print directory
    let MAX_MEMORY_LENGTH = 20
    let DefaultSetting =
            ChatPrompt(
                "system",
                "Play as an stranger in the labyrinth.\
                You don't want to talk with others.\
                You don't want to go anywhere except for staying here.\
                Reply to the user in his voice."
            )
    let DefaultCache =
        [
            ChatPrompt("user", "Hello?")
            ChatPrompt("assistant", "Mind your own business.")
            ChatPrompt("user", "Is it safe around here?")
            ChatPrompt("assistant", "It's none of my business.")
        ]
    let API = "your key here" |> OpenAIClient
    member this.ConversationSettings =
        if Directory.Exists directory then
            Directory.EnumerateDirectories directory
            |> Seq.map (fun path ->
                let name =
                    Path.DirectorySeparatorChar.ToString()
                    |> path.Split 
                    |> Array.last
                    |> fun x -> x.Trim().ToLower()
                let setting =
                    [| path; "examples.txt" |]
                    |> Path.Join
                    |> File.ReadAllText
                    |> fun message -> ChatPrompt("system", message)
                (name, setting)
            )
            |> Map.ofSeq
        else
            failwith "Character settings not found"
            Map.empty<string, ChatPrompt>
     member this.ConversationCache =
        if Directory.Exists directory then
            Directory.EnumerateDirectories directory
            |> Seq.map (fun path ->
                let name =
                    Path.DirectorySeparatorChar.ToString()
                    |> path.Split 
                    |> Array.last
                    |> fun x -> x.Trim().ToLower()
                GD.Print name
                let examples =
                    [|path; "examples.txt"|]
                    |> Path.Join
                    |> File.ReadAllLines
                    |> Seq.ofArray
                    |> Seq.map (fun line -> line.Trim())
                    |> Seq.filter (fun line -> line.Length <> 0)
                    |> Seq.map (fun line ->
                            let [| role; message |] =
                                line.Split(":", StringSplitOptions.TrimEntries)
                                |> Array.take 2
                            ChatPrompt(role, message)
                        )
                    |> List.ofSeq
                KeyValuePair(name, examples)
            )
            |> Dictionary<string, ChatPrompt list>
        else
            Dictionary<string, ChatPrompt list>()
    member this.GetAPIResponse (character: string) (query: string) =
        task {
            let key = character.ToLower()
            let setting = this.ConversationSettings.GetValueOrDefault (key, DefaultSetting)
            let cache = this.ConversationCache.GetValueOrDefault (key, DefaultCache)
            let queryPrompt = ChatPrompt("user", query)
            let! result =
                setting :: cache @ [queryPrompt]
                |> ChatRequest
                |> API.ChatEndpoint.GetCompletionAsync
                |> Async.AwaitTask
                |> Async.Catch // Catch exception due to network error
            match result with
            | Choice1Of2 chatResponse ->
                let response = chatResponse.FirstChoice.Message.Content
                let cache =
                    cache @ [queryPrompt; ChatPrompt("assistant", response)]
                    |> List.skip (cache.Length + 2 - MAX_MEMORY_LENGTH)
                this.ConversationCache[key] <- cache
                return response
            | Choice2Of2 _ ->
                return "(unrecognized mumbling...)"
        }
    