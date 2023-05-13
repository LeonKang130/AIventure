﻿module Communication.ThroughBackend
open System
open System.IO
open System.Collections.Generic
open FSharp.Core
open Microsoft.FSharp.Collections
open FSharp.Json
open FSharp.Data
open Godot
type Prompt = {
    role: string
    content: string
}
let ChatPrompt (role, content) = { role = role; content = content }
type ChatBotHandler(directory: string) =
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
    let ConversationSettings =
        if Directory.Exists directory then
            Directory.EnumerateDirectories directory
            |> Seq.map (fun path ->
                let name =
                    Path.DirectorySeparatorChar.ToString()
                    |> path.Split 
                    |> Array.last
                    |> fun x -> x.Trim().ToLower()
                let setting =
                    [| path; "setting.txt" |]
                    |> Path.Join
                    |> File.ReadAllText
                    |> fun message -> ChatPrompt("system", message.Trim())
                (name, setting)
            )
            |> Map.ofSeq
        else
            failwith "Character settings not found"
            Map.empty<string, Prompt>
    let mutable ConversationCache =
        if Directory.Exists directory then
            Directory.EnumerateDirectories directory
            |> Seq.map (fun path ->
                let name =
                    Path.DirectorySeparatorChar.ToString()
                    |> path.Split 
                    |> Array.last
                    |> fun x -> x.Trim().ToLower()
                let examples =
                    [|path; "examples.txt"|]
                    |> Path.Join
                    |> File.ReadAllLines
                    |> Seq.ofArray
                    |> Seq.map (fun line -> line.Trim())
                    |> Seq.filter (fun line -> line.Length <> 0)
                    |> Seq.map (fun line ->
                            GD.Print line
                            let [| role; message |] =
                                line.Split(":", StringSplitOptions.TrimEntries)
                                |> Array.take 2
                            ChatPrompt(role, message)
                        )
                    |> List.ofSeq
                KeyValuePair(name, examples)
            )
            |> Dictionary<string, Prompt list>
        else
            Dictionary<string, Prompt list>()
    member this.GetAPIResponse (character: string) (query: string) =
        task {
            let key = character.ToLower()
            let setting = ConversationSettings.GetValueOrDefault (key, DefaultSetting)
            let mutable cache = ConversationCache.GetValueOrDefault (key, DefaultCache)
            GD.Print cache.Length
            let queryPrompt = ChatPrompt("user", query)
            let prompts =
                setting :: cache @ [queryPrompt]
                |> Json.serialize
            let! result = Http.AsyncRequest(
                "http://183.172.169.230:3000/chat",
                httpMethod = "POST",
                body = HttpRequestBody.FormValues(
                    [("messages", prompts)]
                    |> Seq.ofList
                ),
                silentHttpErrors = true
            )
            let text =
                match result.Body with
                | Text t ->
                    ConversationCache.Item(key) <-
                        cache @ [queryPrompt; ChatPrompt("assistant", t)]
                        |> List.skip (cache.Length + 2 - MAX_MEMORY_LENGTH)
                    t
                | _ -> "(unrecognized mumbling...)"
            return text
        }