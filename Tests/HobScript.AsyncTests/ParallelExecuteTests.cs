using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HobScript;
using Xunit;

namespace HobScript.AsyncTests
{
    public class ParallelExecuteTests
    {
        [Fact]
        public async Task Execute_SimpleScripts_InParallel_EnginesAreIndependent()
        {
            var e1 = new ScriptEngine();
            var e2 = new ScriptEngine();

            var t1 = e1.ExecuteAsync("a = 3\na");
            var t2 = e2.ExecuteAsync("b = 30\nb");

            await Task.WhenAll(t1, t2);

            Assert.NotNull(e1.GetVariable("a"));
            Assert.NotNull(e2.GetVariable("b"));

            Assert.Null(e1.GetVariable("b"));
            Assert.Null(e2.GetVariable("a"));
        }

        [Fact]
        public async Task ExecuteFile_SimpleFiles_InParallel()
        {
            // Create temp scripts
            var dir = Path.Combine(Path.GetTempPath(), "hobscript_async_tests");
            Directory.CreateDirectory(dir);

            var file1 = Path.Combine(dir, "s1.hob");
            var file2 = Path.Combine(dir, "s2.hob");

            await File.WriteAllTextAsync(file1, "x = 5\ny = 7\nresult = x + y\n");
            await File.WriteAllTextAsync(file2, "m = 2\nn = 4\nres = m * n\n");

            var e1 = new ScriptEngine();
            var e2 = new ScriptEngine();

            var t1 = e1.ExecuteFileAsync(file1);
            var t2 = e2.ExecuteFileAsync(file2);

            var results = await Task.WhenAll(t1, t2);

            Assert.Equal(12d, Convert.ToDouble(results[0]));
            Assert.Equal(8d, Convert.ToDouble(results[1]));
        }

        [Fact]
        public async Task ExecuteAsync_CancelsWhenTokenAlreadyCancelled()
        {
            var engine = new HobScriptEngine();
            var script = "a = 1 + 1";

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await engine.ExecuteAsync(script, cts.Token);
            });
        }
    }
}


