module Communication.SkipBackend
open System
open System.Collections.Generic
open System.IO
open FSharp.Core
open Microsoft.FSharp.Collections
open OpenAI
open OpenAI.Chat

type ChatBotHandler() =
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
    let API = "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" |> OpenAIClient
    member this.ConversationSettings =
        if Directory.Exists "characters" then
            Directory.EnumerateDirectories "characters"
            |> Seq.map (fun path ->
                let name =
                    Path.DirectorySeparatorChar.ToString()
                    |> path.Split 
                    |> Array.last
                    |> fun x -> x.Trim().ToLower()
                let setting =
                    [|path; "examples.txt"|]
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
        if Directory.Exists "characters" then
            Directory.EnumerateDirectories "characters"
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
            let setting = this.ConversationSettings.GetValueOrDefault (character, DefaultSetting)
            let key = character.ToLower()
            let cache = this.ConversationCache.GetValueOrDefault (key, DefaultCache)
            let queryPrompt = ChatPrompt("user", query)
            let! result =
                setting :: cache @ [queryPrompt]
                |> ChatRequest
                |> API.ChatEndpoint.GetCompletionAsync
            let response = result.FirstChoice.Message.Content
            let cache =
                cache @ [queryPrompt; ChatPrompt("assistant", response)]
                |> List.skip (cache.Length + 2 - MAX_MEMORY_LENGTH)
            this.ConversationCache[key] <- cache
            return response
        }
    