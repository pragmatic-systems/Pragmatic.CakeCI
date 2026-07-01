using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;

namespace Pragsys.CakeCI;

[CakeAliasCategory("SonarQube")]
public static class SonarResults
{
    [CakeMethodAlias]
    [CakeAliasCategory("Scan Check")]
    public static async Task SonarScanCheck(this ICakeContext context)
    {
        ICakeLog logger = context.Log;
        logger.Information("=========================================");
        logger.Information("SONAR SCAN CHECK SHIP");
        logger.Information("=========================================");
    }
}