namespace ParallelBenchmarks
open System
open FsCheck
open Newtonsoft.Json.Linq
open Newtonsoft.Json

module Gen =
    let subListOf (elems :array<'a>) min =
        gen {
            let! size = Gen.choose(min, elems.Length)
            let! indices = Gen.listOfLength size (Gen.choose(0, elems.Length-1)) 
            let subSeq = indices |> Seq.distinct |> Seq.map (fun i -> elems.[i])
        return List.ofSeq subSeq }       


module Generators =

    let isPrime (n :int32) = 
        let mutable res = true
        if n <= 1 then res <- false
        else if n <= 3 then res <- true
        else if n % 2 = 0 || n % 3 = 0 then res <- false
        else
            let mutable i = 5
            while i * i <= n do
                if n % i = 0 || n % (i + 2) = 0 then
                    res <- false
                else i <- i + 6
        res

    let nextPrime n =
        let mutable k = n
        while not <| isPrime k do
            k <- k + 1
        k

    let primes = Arb.generate<int32> |> Gen.map nextPrime

    type PrimeGen =
        static member PrimeGen() = {
            new Arbitrary<int32>() with
                override x.Generator = primes
                override x.Shrinker t = Seq.empty }

    let len = Gen.choose (6, 27)
    
    let validPropChars =
        Arb.Default.Char () |> Arb.filter (fun c -> 'A' <= c && c <= 'Z') |> Arb.toGen
        
    let names = len >>= (fun i -> Gen.arrayOfLength i validPropChars) |> Gen.map String

    let dateSerializer = new JsonSerializer ()
    dateSerializer.DateParseHandling <- DateParseHandling.None

    let strs = Arb.generate<string> |> Gen.where (isNull >> not) |> Gen.map JToken.FromObject
    let ints = Arb.generate<int> |> Gen.map JToken.FromObject
    let doubles = Arb.generate<float> |> Gen.map (fun f ->  f.ToString () |> JToken.FromObject)
    let dates = Arb.generate<DateTime> |> Gen.map (fun d -> JToken.FromObject (d.ToString("s"), dateSerializer))
    let guids = Arb.generate<Guid> |> Gen.map JToken.FromObject
    let intLists = Arb.generate<int[]> |> Gen.where (isNull >> not) |> Gen.map JToken.FromObject
    let doubleLists = Arb.generate<float[]> |> Gen.where (isNull >> not) |> Gen.map JToken.FromObject
    let strLists = Arb.generate<string[]> |> Gen.where (isNull >> not) |> Gen.map JToken.FromObject
    let dateLists = Arb.generate<DateTime[]> |> Gen.where (isNull >> not) |> Gen.map (fun dx -> JToken.FromObject (dx, dateSerializer))
    let guidLists = Arb.generate<Guid[]> |> Gen.where (isNull >> not) |> Gen.map JToken.FromObject

    
    let types = 
        [| (10, strs); (2, ints); (2, doubles); (2, dates); (2, guids); (1, intLists); (1, doubleLists); (1, strLists); (1, dateLists); (1, guidLists) |]
        |> Array.collect (fun (i, g) -> Array.replicate i g)
    let typesLen = Array.length types

    let jsons = gen {
        let! fieldNames = names |> Gen.nonEmptyListOf
        let fieldsPool :(string * Gen<JToken>) array = Array.zeroCreate fieldNames.Length 

        for i in 0..fieldNames.Length-1 do 
            let! j = Gen.choose (0, typesLen-1)
            fieldsPool.[i] <- (fieldNames.[i], types.[j])
            
        let! n = Gen.choose (1, 2000)
        let jsonGens = List.init n (fun _ -> Gen.subListOf fieldsPool 1)
        let jsons = jsonGens |> List.map (fun fieldsGen -> gen {
            let jobj = new JObject()
            let! fields = fieldsGen 
            for (f, g) in fields do
                let! v = g
                jobj.[f] <- v
            let! enqD = Arb.generate<DateTime> |> Gen.map (fun d -> JToken.FromObject (d.ToString("s"), dateSerializer))
            jobj.["TIME1"] <- enqD
            let! procD = Arb.generate<DateTime> |> Gen.map (fun d -> JToken.FromObject (d.ToString("s"), dateSerializer))
            jobj.["TIME2"] <- procD
            return jobj
        })
        return! jsons |> Gen.sequence
    }

    type JsonGen =
        static member JsonGen() = {
            new Arbitrary<list<JObject>>() with
                override x.Generator = jsons
                override x.Shrinker t = Seq.empty }