using Volo.Abp.GlobalFeatures;
using Volo.Abp.Threading;

namespace Informatique.Alumni;

public static class AlumniGlobalFeatureConfigurator
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        OneTimeRunner.Run(() =>
        {
                /* You can configure (enable/disable) global features of the used modules here.
                 * Please refer to the documentation to learn more about the Global Features System:
                 * https://docs.abp.io/en/abp/latest/Global-Features
                 */
            });
    }
}
