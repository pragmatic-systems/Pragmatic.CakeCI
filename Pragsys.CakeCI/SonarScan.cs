using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;

namespace Pragsys.CakeCI;

[CakeAliasCategory("PragsysCI")]
public static class PragsysCI
{
    [CakeMethodAlias]
    [CakeAliasCategory("Scan Check")]
    public static async Task SonarScanCheck(this ICakeContext context)
    {
        ICakeLog logger = context.Log;
        logger.Information("=========================================");
        logger.Information("SONAR SCAN CHECK SHIP");
        logger.Information("=========================================");
        await Task.Delay(100);
    }
}