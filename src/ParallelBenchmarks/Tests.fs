namespace ParallelBenchmarks
open Properties
open FsCheck
open Newtonsoft.Json.Linq
open Newtonsoft.Json

open Generators

module Tests =
    
    let ``Csv lines count equals jsons count`` config =
        Arb.register<JsonGen> () |> ignore
        Check.One (config, linesCount)

    let ``Csv all rows same width`` config =
        Arb.register<JsonGen> () |> ignore
        Check.One (config, allFields)

    let ``Csv all rows equals corresponding jobj`` config =
        Arb.register<JsonGen> () |> ignore
        Check.One (config, sameFieldsAsJobj)

    let primesCrunch config =
        Arb.register<PrimeGen> () |> ignore
        Check.One (config, Generators.isPrime)

    let ``sort should give same result as insertion sort`` config =    
        Check.One (config, sameSort)

    let ``io work emulation`` config =
        Check.One (config, ioWork)

