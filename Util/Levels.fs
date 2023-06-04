module Util.Levels
open System
open System.IO
open System.Net
open System.Text.Json
open Godot
open FSharp.Data


type Item =
    | Empty
    | ItemDesc of string * string * Func<Texture2D>
    member this.name =
        match this with
        | Empty -> "crystal ball"
        | ItemDesc(name, _, _) -> name
    member this.description =
        match this with
        | Empty -> "A crystal ball that seems to have magical power. It may be able to shelter you from danger."
        | ItemDesc(_, description, _) -> description
    member this.imageTexture =
        match this with
        | ItemDesc(_, _, func) -> func.Invoke()
        | Empty ->
            "res://temp/crystal ball.png"
            |> Image.LoadFromFile
            |> ImageTexture.CreateFromImage
            |> fun x -> upcast x : Texture2D
        

type Level =
    | Empty
    | Trap
    | Treasure of Item
    | NPC of string
    | Enemy of string

type NetworkItem = {
    id: int
    name: string
    description: string
    url: string
}

let MapWidth, MapHeight = 10, 10
let MinJourneyLength = 6
let TrapNum = 5
let TreasureNum = 20
let InventoryCapacity = 20
let VisionWidth = 2
let NPCNum = 10

type LevelHandler(characterList: string list) =
    let mutable itemPool =
        let directory =
            if OS.HasFeature("editor") then
                ProjectSettings.GlobalizePath("items")
            else
                OS.GetExecutablePath().GetBaseDir().PathJoin("items")
        if not (directory |> Directory.Exists) then
            directory |> Directory.CreateDirectory |> ignore
        let fallbackTexture =
            ResourceLoader.Load<Texture2D>("res://arts/crystal ball.png")
        let fallbackItem = (
            "crystal ball",
            "A crystal ball that seems to have magical power. It may be able to shelter you from danger.",
            Func<Texture2D>(fun () -> upcast fallbackTexture : Texture2D)
        )
        let response = Http.Request(
            "http://43.153.90.127:3000/image",
            httpMethod = "POST",
            body = HttpRequestBody.FormValues [("items_num", TreasureNum.ToString())]
        )
        match response.Body with
        | Text t ->
            let items =
                let networkItems =
                    t
                    |> JsonSerializer.Deserialize<NetworkItem list>
                networkItems |> GD.Print
                let textureTasks =
                    networkItems
                    |> List.distinctBy (fun networkItem -> networkItem.id)
                    |> List.sortBy (fun networkItem -> networkItem.id)
                    |> List.map (fun networkItem ->
                        let filename =
                            [| directory; $"{networkItem.id}.png" |]
                            |> Path.Join
                        let uri = Uri($"http://{networkItem.url}")
                        let textureTask =
                            task {
                                let client = WebClient()
                                do! client.DownloadFileTaskAsync(uri, filename)
                                return filename
                                    |> Image.LoadFromFile
                                    |> ImageTexture.CreateFromImage
                            }
                        (networkItem.id, textureTask)
                    )
                    |> Map.ofList
                networkItems
                |> List.map (fun networkItem ->
                    let textureTask = textureTasks.Item networkItem.id
                    (
                        networkItem.name,
                        networkItem.description,
                        Func<Texture2D>(fun () ->
                            if textureTask.IsCompleted then upcast textureTask.Result : Texture2D
                            else fallbackTexture
                        )
                    )    
                )
            items @ [ for _ in 0 .. TreasureNum - items.Length - 1 -> fallbackItem ]
        | _ -> [for _ in 0 .. TreasureNum - 1 -> fallbackItem]
        
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
        for i in 0 .. TreasureNum - 1 do
            let mutable location = Vector2I(random.Next(MapWidth - 1), random.Next(MapHeight - 1))
            while grid[location.X][location.Y] <> Empty do
                location <- Vector2I(random.Next(MapWidth - 1), random.Next(MapHeight - 1))
            let item = ItemDesc <| List.head itemPool
            itemPool <- List.tail itemPool
            grid[location.X][location.Y] <- Level.Treasure(item)
            GD.Print $"Treasure({item.ToString()}) At: {location.ToString()}"
        for i in 0 .. NPCNum - 1 do
            let mutable NPCIndex = random.Next(characterList.Length)
            while characterList[NPCIndex] = "Devil" or characterList[NPCIndex] = "Harold" do
                NPCIndex <- random.Next(characterList.Length)
            let mutable location = Vector2I(random.Next(MapWidth - 1), random.Next(MapHeight - 1))
            while grid[location.X][location.Y] <> Empty do
                location <- Vector2I(random.Next(MapWidth - 1), random.Next(MapHeight - 1))
            GD.Print $"Spawned {characterList[NPCIndex]} at {location}"
            grid[location.X][location.Y] <- NPC(characterList[NPCIndex])
        grid
    let mutable visited =
        let mutable visited =
            [| for _ in 0 .. MapWidth - 1 -> [| for _ in 0 .. MapHeight - 1 -> false |] |]
        visited[spawn.X][spawn.Y] <- true
        visited
    let mutable current = spawn
    member this.GroundTruthInfo =
        seq {
            for i in current.X - VisionWidth .. current.X + VisionWidth do
                for j in current.Y - VisionWidth .. current.Y + VisionWidth -> (i, j)
        }
        |> Seq.filter (fun (i, j) -> i >= 0 && i < MapWidth && j >= 0 && j < MapHeight && grid[i][j] = Trap)
        |> Seq.map(fun (i, j) -> (i - current.X, j - current.Y))
        |> Seq.map (fun(i, j) ->
            let directionH =
                if i <= 0 then "West" else "East"
            let directionV =
                if j <= 0 then "North" else "South"
            match (i, j) with
                | 0, deltaY -> $"{abs deltaY} blocks {directionV}"
                | deltaX, 0 -> $"{abs deltaX} blocks {directionH}"
                | deltaX, deltaY -> $"{abs deltaY} blocks {directionV} and {abs deltaX} blocks {directionH}"
        )
        |> Seq.map (fun x -> $"There is a trap {x} from here.")
        |> String.concat " "
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
    member this.CurrentLevelTreasureContent =
        match this.CurrentLevel with
        | Level.Treasure item -> item
        | _ -> Item.Empty
    member this.RemoveCurrentLevelTreasureContent() =
        match this.CurrentLevel with
        | Level.Treasure _ -> grid[current.X][current.Y] <- Treasure Item.Empty
        | _ -> ()
    member this.RemoveCurrentLevelTrap() =
        match this.CurrentLevel with
        | Level.Trap -> grid[current.X][current.Y] <- Empty
        | _ -> ()