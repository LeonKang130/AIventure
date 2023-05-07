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
let TrapNum = 25

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
    let DestinationReachable (grid: Level array array) (spawn: Vector2I) (destination: Vector2I) =
        let mutable visited =
            [| for _ in 0 .. MapWidth - 1 -> [| for _ in 0 .. MapHeight - 1 -> false |] |]
        visited[spawn.X][spawn.Y] <- true
        let mutable buffer = [spawn]
        while buffer.Length <> 0 && (visited[destination.X][destination.Y] = false) do
            let curr = List.head buffer
            buffer <- List.tail buffer
            if curr.X <> 0 && grid[curr.X - 1][curr.Y] <> Trap && not (visited[curr.X - 1][curr.Y]) then
                buffer <- buffer @ [Vector2I(curr.X - 1, curr.Y)]
                visited[curr.X - 1][curr.Y] <- true
            if curr.X <> MapWidth - 1 && grid[curr.X + 1][curr.Y] <> Trap && not (visited[curr.X + 1][curr.Y]) then
                buffer <- buffer @ [Vector2I(curr.X + 1, curr.Y)]
                visited[curr.X + 1][curr.Y] <- true
            if curr.Y <> 0 && grid[curr.X][curr.Y - 1] <> Trap && not (visited[curr.X][curr.Y - 1]) then
                buffer <- buffer @ [Vector2I(curr.X, curr.Y - 1)]
                visited[curr.X][curr.Y - 1] <- true
            if curr.Y <> MapHeight - 1 && grid[curr.X][curr.Y + 1] <> Trap && not (visited[curr.X][curr.Y + 1]) then
                buffer <- buffer @ [Vector2I(curr.X, curr.Y + 1)]
                visited[curr.X][curr.Y + 1] <- true
        visited[destination.X][destination.Y]
    let grid =
        let mutable grid =
           [| for _ in 0 .. MapWidth - 1 -> [| for _ in 0 .. MapHeight - 1 -> Level.Empty |] |]
        // map initialization here
        grid[spawn.X][spawn.Y] <- Level.NPC("Harold")
        grid[destination.X][destination.Y] <- Level.NPC("Devil")
        for i in 0 .. TrapNum - 1 do
            let mutable location = Vector2I(random.Next(MapWidth - 1), random.Next(MapHeight - 1))
            while grid[location.X][location.Y] <> Empty do
                location <- Vector2I(random.Next(MapWidth - 1), random.Next(MapHeight - 1))
            grid[location.X][location.Y] <- Trap
            while not (DestinationReachable grid spawn destination) do
                grid[location.X][location.Y] <- Empty
                location <- Vector2I(random.Next(MapWidth - 1), random.Next(MapHeight - 1))
                while grid[location.X][location.Y] <> Empty do
                location <- Vector2I(random.Next(MapWidth - 1), random.Next(MapHeight - 1))
                grid[location.X][location.Y] <- Trap
            GD.Print $"Trap At: {location.ToString()}"
        grid
    let mutable visited =
        let mutable visited =
            [| for _ in 0 .. MapWidth - 1 -> [| for _ in 0 .. MapHeight - 1 -> false |] |]
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