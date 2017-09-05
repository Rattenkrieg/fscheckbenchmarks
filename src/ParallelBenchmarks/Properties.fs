namespace ParallelBenchmarks
open System
open FsCheck
open System.Text
open System.IO
open Newtonsoft.Json.Linq
open Newtonsoft.Json
open System.Collections.Generic
open System.Linq
open CsvHelper
open CsvHelper.Configuration

open Generators
open BitCruncher

module InsertionSort =

    let rec insert (newElem :int) list = 
        match list with 
        | head::tail when newElem > head -> 
            head :: insert newElem tail
        | other ->
            newElem :: other 

    let rec sort (list :int list) = 
        match list with
        | []   -> []
        | head::tail -> 
            insert head (sort tail)

module Properties =   

    let jobjToEventData (jobj :JObject) =
        use stream = new MemoryStream ()
        use swriter = new StreamWriter (stream)
        use jwriter = new JsonTextWriter (swriter)
        jobj.WriteTo jwriter
        jwriter.Flush ()
        let ev = new EventData (stream.ToArray ())
        ev


    let leetToCsv = DataManipulation.ChunkToCsv


    let linesCount (jx :list<JObject>) =
        let events = jx |> List.map jobjToEventData
        use leetCsv = events |> leetToCsv
        leetCsv.Seek (0L, SeekOrigin.Begin) |> ignore
        use leetReader = new StreamReader (leetCsv)
        let mutable lineCnt = -1
        let cfg = new CsvConfiguration ()
        cfg.IgnoreBlankLines <- false
        cfg.SkipEmptyRecords <- false
        cfg.DetectColumnCountChanges <- true
        use parser = new CsvParser (leetReader, cfg)
        while parser.Read () |> isNull |> not do
            lineCnt <- lineCnt + 1
        let jxLen = List.length jx
        if jxLen <> lineCnt then printfn "%A" jxLen
        jxLen = lineCnt
        

    let allFields (jx :list<JObject>) =
        let events = jx |> List.map jobjToEventData
        use leetCsv = events |> leetToCsv
        leetCsv.Seek (0L, SeekOrigin.Begin) |> ignore
        use leetReader = new StreamReader (leetCsv)
        let csvReader = new CsvReader (leetReader)
        csvReader.ReadHeader () |> ignore
        let fieldCnt = csvReader.FieldHeaders.Count ()
        while csvReader.Read () do
            for i in 0..fieldCnt-1 do
                csvReader.GetField<string> i |> ignore
        true
    

    let jtokToString (jt :JToken) =
        if isNull jt then "" else
        use strWriter = new StringWriter ()
        use jWriter = new JsonTextWriter (strWriter)
        jt.WriteTo jWriter
        jWriter.Flush ()
        strWriter.ToString().Replace("\"", "")


    let jtokenToString (jobj :JObject) (prop :string) =
        let jt = jobj.GetValue prop
        match jt with
        | :? JArray as ja ->
            ja
            |> Seq.map (fun (v :JToken) -> 
                match v with
                | :? JValue as jv -> 
                    match jv.Type with
                    | JTokenType.Date -> (jv.Value :?> DateTime).ToString ("s")
                    | JTokenType.String -> v.Value<string> ()
                    | JTokenType.Float -> jtokToString v
                    | JTokenType.Null -> ""
                    | _ -> jv.Value.ToString ()
                | _ -> failwith "only flat"
            )
            |> fun xs -> "[" + String.Join (",", xs) + "]"
        | :? JValue as jv -> 
            if jv.Type = JTokenType.Date then
                jt.Value<DateTime>().ToString("s")
            else
                JToken.op_Explicit jt
        | null -> ""
        | _ -> failwith "no way"
        

    let sameFieldsAsJobj (jx :list<JObject>) =
        let events = jx |> List.map jobjToEventData
        use leetCsv = events |> leetToCsv
        leetCsv.Seek (0L, SeekOrigin.Begin) |> ignore
        use leetReader = new StreamReader (leetCsv)
        let cfg = new CsvConfiguration ()
        cfg.IgnoreBlankLines <- false
        cfg.DetectColumnCountChanges <- true
        let csvReader = new CsvReader (leetReader, cfg)
        csvReader.ReadHeader () |> ignore
        let fieldCnt = csvReader.FieldHeaders.Count ()
        let lel = jx |> List.forall (fun j ->
            if csvReader.Read () |> not then failwith "no way"
            let mutable acc = true
            for i in 0..fieldCnt-1 do
                let field = csvReader.GetField<string> i
                if field |> isNull |> not then
                    let jval = jtokenToString j (csvReader.FieldHeaders.[i])
                    acc <- acc && (field = jval)
                else failwith "no way"
            acc
        )
        lel

    let sameSort (aList:int list) =    
        let sorted1 = aList |> List.sortWith (fun a b -> a.CompareTo b)
        let sorted2 = aList |> InsertionSort.sort     
        sorted1.SequenceEqual(sorted2, Collections.Generic.EqualityComparer<int32>.Default)

    let ioWork (i :int) =
        let i = Math.Abs i        
        let t = System.Threading.Tasks.Task.Delay i
        t.ContinueWith (fun _ -> true)

    let ioTrueWork (i :int) =
        async {
            let i = Math.Abs i        
            let c = new System.Net.Http.HttpClient ()
            let x = c.GetAsync ("https://google.com") // /search?q=" + string i)
            let! z = Async.AwaitTask x
            let! q = z.Content.ReadAsStringAsync () |> Async.AwaitTask
            return q.Length > 0
        }

    let cpuWork (x :float) (y :float) =
        let mutable a = 0.0
        let z = x |> Math.Abs |> Math.Ceiling |> Math.Log
        for i in 0..(if Double.IsNaN z || Double.IsInfinity z then 5 else z |> Convert.ToInt32) do
            a <- a + Math.Atan2 (x, y)
        a <> (a - 0.1)
