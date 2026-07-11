// No-op benchmark that generates a marker artifact file. This file exists only to test the Benchmark flow during build.cake execution.
var artifactsDir = args.Length >= 2 && args[0] == "--artifacts" ? args[1] : null;

if (!string.IsNullOrEmpty(artifactsDir))
{
    Directory.CreateDirectory(artifactsDir);
    var markerPath = Path.Combine(artifactsDir, "benchmark-results.json");
    File.WriteAllText(markerPath, """
        {
          "run": "no-op",
          "timestamp": "2026-01-01T00:00:00Z",
          "benchmarks": []
        }
        """);
    Console.WriteLine($"Benchmark artifacts written to {artifactsDir}");
}
else
{
    Console.WriteLine("No-op benchmark completed (no artifacts directory specified).");
}
