// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running
open BenchmarkDotNet.Jobs
open BenchmarkDotNet.Core
open BenchmarkDotNet.Configs
open BenchmarkDotNet
open BenchmarkDotNet.Attributes.Jobs
open BenchmarkDotNet.Environments
open FsCheck
open System

type MyConfig() =
    inherit ManualConfig()
    let baseJob = 
        Job.Dry 
            .With(Runtime.Clr)
            .WithLaunchCount(4)
            .WithUnrollFactor(1)
            .WithInvocationCount(1)
            .WithTargetCount(1)
            .WithWarmupCount(0)
    do 
        base.Add(baseJob
        .With(Platform.X64)
        .With(Jit.RyuJit))
        base.Add(baseJob
        .With(Platform.X64)
        .With(Jit.LegacyJit))
        base.Add(baseJob
        .With(Platform.X86)
        .With(Jit.RyuJit))
        base.Add(baseJob
        .With(Platform.X86)
        .With(Jit.LegacyJit))
        
        base.Add(baseJob
        .With(Platform.X64).WithGcServer(true).WithGcConcurrent(true)
        .With(Jit.RyuJit))
        base.Add(baseJob
        .With(Platform.X64).WithGcServer(true).WithGcConcurrent(true)
        .With(Jit.LegacyJit))
        base.Add(baseJob
        .With(Platform.X86).WithGcServer(true).WithGcConcurrent(true)
        .With(Jit.RyuJit))
        base.Add(baseJob
        .With(Platform.X86).WithGcServer(true).WithGcConcurrent(true)
        .With(Jit.LegacyJit))
        
        
[<Config(typeof<MyConfig>)>]
type FsCheckParallelBenchIo () =

    let config = { Config.QuickThrowOnFailure with Replay = Rnd(10538531436017130025UL,14826463994991344553UL) |> Some; QuietOnSuccess = true; EndSize = 1000; MaxTest = 100 }

    [<Benchmark>]
    member self.SeqIo () = 
        let config = { config with ParallelRunConfig = None }
        ParallelBenchmarks.Tests.``io work emulation`` config

    [<Benchmark>]
    member self.DedicatedParallelIo16 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 16 } }
        ParallelBenchmarks.Tests.``io work emulation`` config

    [<Benchmark>]
    member self.DedicatedParallelIo8 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 8 } }
        ParallelBenchmarks.Tests.``io work emulation`` config

    [<Benchmark>]
    member self.DedicatedParallelIo4 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 4 } }
        ParallelBenchmarks.Tests.``io work emulation`` config
        

[<Config(typeof<MyConfig>)>]
type FsCheckParallelBenchTrueIo () =

    let config = { Config.QuickThrowOnFailure with Replay = Rnd(10538531436017130025UL,14826463994991344553UL) |> Some; QuietOnSuccess = true; EndSize = 1000; MaxTest = 100 }

    [<Benchmark>]
    member self.SeqIo () = 
        let config = { config with ParallelRunConfig = None }
        ParallelBenchmarks.Tests.``true io work emulation`` config

    [<Benchmark>]
    member self.DedicatedParallelIo16 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 16 } }
        ParallelBenchmarks.Tests.``true io work emulation`` config

    [<Benchmark>]
    member self.DedicatedParallelIo8 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 8 } }
        ParallelBenchmarks.Tests.``true io work emulation`` config

    [<Benchmark>]
    member self.DedicatedParallelIo4 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 4 } }
        ParallelBenchmarks.Tests.``true io work emulation`` config


[<Config(typeof<MyConfig>)>]
type FsCheckParallelBenchSort () =

    let config = { Config.QuickThrowOnFailure with Replay = Rnd(10538531436017130025UL,14826463994991344553UL) |> Some; QuietOnSuccess = true; EndSize = 1000; MaxTest = 100000 }

    [<Benchmark>]
    member self.SeqSort () = 
        let config = { config with ParallelRunConfig = None }
        ParallelBenchmarks.Tests.``sort should give same result as insertion sort`` config

    [<Benchmark>]
    member self.DedicatedParallelSort16 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 16 } }
        ParallelBenchmarks.Tests.``sort should give same result as insertion sort`` config

    [<Benchmark>]
    member self.DedicatedParallelSort8 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 8 } }
        ParallelBenchmarks.Tests.``sort should give same result as insertion sort`` config

    [<Benchmark>]
    member self.DedicatedParallelSort4 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 4 } }
        ParallelBenchmarks.Tests.``sort should give same result as insertion sort`` config

        
[<Config(typeof<MyConfig>)>]
type FsCheckParallelBenchPrimes () =

    let config = { Config.QuickThrowOnFailure with Replay = Rnd(10538531436017130025UL,14826463994991344553UL) |> Some; QuietOnSuccess = true; }

    [<Benchmark>]
    member self.SeqPrimes () = 
        let config = { config with ParallelRunConfig = None }
        ParallelBenchmarks.Tests.``Csv all rows equals corresponding jobj`` config
        
    [<Benchmark>]
    member self.DedicatedParallelPrimes16 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 16 } }
        ParallelBenchmarks.Tests.``Csv all rows equals corresponding jobj`` config
        
    [<Benchmark>]
    member self.DedicatedParallelPrimes8 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 8 } }
        ParallelBenchmarks.Tests.``Csv all rows equals corresponding jobj`` config
        
    [<Benchmark>]
    member self.DedicatedParallelPrimes4 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 4 } }
        ParallelBenchmarks.Tests.``Csv all rows equals corresponding jobj`` config


[<Config(typeof<MyConfig>)>]
type FsCheckParallelBench () =

    let config = { Config.QuickThrowOnFailure with Replay = Rnd(10538531436017130025UL,14826463994991344553UL) |> Some; QuietOnSuccess = true }

    [<Benchmark>]
    member self.DedicatedParallelCount16 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 16 } }
        ParallelBenchmarks.Tests.``Csv lines count equals jsons count`` config

    [<Benchmark>]
    member self.DedicatedParallelCount8 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 8 } }
        ParallelBenchmarks.Tests.``Csv lines count equals jsons count`` config

    [<Benchmark>]
    member self.DedicatedParallelCount4 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 4 } }
        ParallelBenchmarks.Tests.``Csv lines count equals jsons count`` config

    [<Benchmark>]
    member self.SeqCount () = 
        let config = { config with ParallelRunConfig = None }
        ParallelBenchmarks.Tests.``Csv lines count equals jsons count`` config
        
    [<Benchmark>]
    member self.DedicatedParallelWidth16 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 16 } }
        ParallelBenchmarks.Tests.``Csv all rows same width`` config
        
    [<Benchmark>]
    member self.DedicatedParallelWidth8 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 8 } }
        ParallelBenchmarks.Tests.``Csv all rows same width`` config
        
    [<Benchmark>]
    member self.DedicatedParallelWidth4 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 4 } }
        ParallelBenchmarks.Tests.``Csv all rows same width`` config

    [<Benchmark>]
    member self.SeqWidth () = 
        let config = { config with ParallelRunConfig = None }
        ParallelBenchmarks.Tests.``Csv all rows same width`` config
        
    [<Benchmark>]
    member self.DedicatedParallelEquals16 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 16 } }
        ParallelBenchmarks.Tests.``Csv all rows equals corresponding jobj`` config
        
    [<Benchmark>]
    member self.DedicatedParallelEquals8 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 8 } }
        ParallelBenchmarks.Tests.``Csv all rows equals corresponding jobj`` config
        
    [<Benchmark>]
    member self.DedicatedParallelEquals4 () = 
        let config = { config with ParallelRunConfig = Some { MaxDegreeOfParallelism = 4 } }
        ParallelBenchmarks.Tests.``Csv all rows equals corresponding jobj`` config

    [<Benchmark>]
    member self.SeqEquals () = 
        let config = { config with ParallelRunConfig = None }
        ParallelBenchmarks.Tests.``Csv all rows equals corresponding jobj`` config


let defaultSwitch () = BenchmarkSwitcher [| typeof<FsCheckParallelBench>; typeof<FsCheckParallelBenchPrimes>; typeof<FsCheckParallelBenchSort>; typeof<FsCheckParallelBenchIo> ; typeof<FsCheckParallelBenchTrueIo> |]
    
[<EntryPoint>]
let Main args =
    defaultSwitch().Run args |> ignore
    0
