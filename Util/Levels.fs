module Util.Levels
open System
open Godot

type Level =
    | Empty
    | Trap
    | Treasure
    | NPC of string
    | Enemy of string
    | Destination

let MapWidth, MapHeight = 10, 10
let MinJourneyLength = 6

type LevelHandler() =
    let random = Random()
    let spawn = Vector2I(random.Next(MapWidth - 1), random.Next(MapHeight - 1))
    let destination =
        let mutable location = Vector2I(random.Next(MapWidth - 1), random.Next(MapHeight - 1))
        let ManhattanDistance (a: Vector2I) (b: Vector2I) =
            let offset = (a - b).Abs()
            offset.X + offset.Y
        while (location |> ManhattanDistance spawn) < MinJourneyLength do
            location <- Vector2I(random.Next(MapWidth - 1), random.Next(MapHeight - 1))
        location
    let grid =
        let mutable grid =
           [| for _ in 0 .. MapWidth - 1 -> [| for _ in 0 .. MapHeight - 1 -> Level.Empty |] |]
        // map initialization here
        grid[spawn.X][spawn.Y] <- Level.NPC("Harold")
        grid[destination.X][destination.Y] <- Level.NPC("Devil")
        grid
    let mutable visited =
        let mutable visited =
            false
            |> Array.create MapHeight
            |> Array.create MapWidth
        visited[spawn.X][spawn.Y] <- true
        visited
    let mutable current = spawn
    member this.CurrentLocation =
        current
    member this.Destination =
        destination
    member this.MapVisited = visited
    member this.MapGrid = grid
    member this.PortalUsability =
        [|
            current.Y > 0
            current.X < MapWidth - 1
            current.Y < MapHeight - 1
            current.X > 0
        |]
    member this.SetLocation (location: Vector2I) =
        current <- location
    member this.CurrentLevel =
        grid[current.X][current.Y]
    member this.CurrentLevelNPCName =
        match this.CurrentLevel with
        | Level.NPC(name) -> name
        | _ -> null