# Minimum Viable Factory

A clone-and-run **.NET 8** hello world for a multi-agent software factory. Semantic Kernel is the brain. The default brain is a deterministic mock, so **no API keys** are required.

Open this repo in Cursor from GitHub: [InfinityCapital7/minimum-viable-factory](https://github.com/InfinityCapital7/minimum-viable-factory).

```
ticket JSON  →  Triage → Plan → Build → Review → Done
                     WorkItem handoff between IFactoryAgent stages
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## How to run

```bash
git clone https://github.com/InfinityCapital7/minimum-viable-factory
cd minimum-viable-factory
dotnet run --project src/SoftwareFactory
```

That command reads `tickets/sample-ticket.json` (id **SF-101**), runs the pipeline, and writes artifacts under `output/`.

Pass another ticket file if you want:

```bash
dotnet run --project src/SoftwareFactory -- tickets/sample-ticket.json
```

## What success looks like

Console logs for each stage (`Triage`, `Plan`, `Build`, `Review`, `Done`). The factory exits 0 and prints:

```text
Verdict: PASS
Factory finished: Review PASS.
```

On disk:

| Path | What it is |
| --- | --- |
| `output/HelloWorld/` | Real `net8.0` console project written by the Build stage |
| `output/factory-status.json` | Run record: ticket id, `review: PASS`, stage history |

Prove the generated app yourself:

```bash
dotnet run --project output/HelloWorld
# Hello, World! Built by the software factory for SF-101.
```

`dotnet build` at the repo root builds the factory. Neither command needs secrets.

## Optional live model

If `OPENAI_API_KEY` is set, the factory uses an OpenAI-compatible chat completion instead of the mock. Optional:

- `OPENAI_MODEL` (default `gpt-4o-mini`)
- `OPENAI_ENDPOINT` (custom OpenAI-compatible base URL)

The pipeline still writes the same HelloWorld project and still PASSes only when that project builds and greets with the ticket id.

## Next step

Swap the fake JSON tickets for **GitHub Issues**. Keep `IFactoryAgent` + `FactoryPipeline` + `WorkItem` the same; change only how a ticket is loaded (issue number → title/body → the same handoff object).

## Layout

```text
tickets/sample-ticket.json     Fake intake (SF-101)
src/SoftwareFactory/           Factory console app
  IFactoryAgent.cs
  FactoryPipeline.cs
  WorkItem.cs
  KernelFactory.cs             Mock brain, or OpenAI if a key is set
  Agents/                      Triage, Plan, Build, Review, Done
output/                        Generated HelloWorld + factory-status.json
```

## License

MIT
