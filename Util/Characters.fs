module Util.Characters
open System.IO
open Godot

type CharacterManager(directory: string) =
    member this.CharacterList =
        if Directory.Exists directory then
            Directory.EnumerateDirectories directory
            |> Seq.map (fun path ->
                Path.DirectorySeparatorChar.ToString()
                |> path.Split
                |> Array.last
                |> fun x -> x.Trim()
            )
            |> List.ofSeq
        else
            []
    member this.CharacterPortraits =
        this.CharacterList
        |> Seq.ofList
        |> Seq.map (fun name ->
            let portrait =
                [| directory; name; $"{name.ToLower()}.png" |]
                |> Path.Join
                |> Image.LoadFromFile
                |> ImageTexture.CreateFromImage
            name.ToLower(), portrait
        )
        |> Map.ofSeq
