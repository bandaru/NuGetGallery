﻿using Moq;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Xunit;
using System.Globalization;
using System.Web;
using System.Threading;
using System.Web.Routing;

namespace NuGetGallery
{
    public class StatisticsControllerFacts
    {
        [Fact]
        public async void StatisticsHomePage_ValidateReportStructureAndAvailability()
        {
            var fakePackageReport = "[{\"PackageId\":\"A\",\"Downloads\":1},{\"PackageId\":\"B\",\"Downloads\":2}]";
            var fakePackageVersionReport = "[{\"PackageId\":\"A\",\"PackageVersion\":\"1.0\",\"Downloads\":3},{\"PackageId\":\"A\",\"PackageVersion\":\"1.1\",\"Downloads\":4},{\"PackageId\":\"B\",\"PackageVersion\":\"1.0\",\"Downloads\":5}]";

            var fakeReportService = new Mock<IReportService>();

            fakeReportService.Setup(x => x.Load("RecentPopularity.json")).Returns(Task.FromResult(fakePackageReport));
            fakeReportService.Setup(x => x.Load("RecentPopularityDetail.json")).Returns(Task.FromResult(fakePackageVersionReport));

            var controller = new StatisticsController(new JsonStatisticsService(fakeReportService.Object), null);

            var model = (StatisticsPackagesViewModel)((ViewResult) await controller.Index()).Model;

            int sum = 0;

            if (model.IsDownloadPackageAvailable)
            {
                foreach (var item in model.DownloadPackagesSummary)
                {
                    sum += item.Downloads;
                }
            }

            if (model.IsDownloadPackageDetailAvailable)
            {
                foreach (var item in model.DownloadPackageVersionsSummary)
                {
                    sum += item.Downloads;
                }
            }

            Assert.Equal<int>(15, sum);
        }

        [Fact]
        public async void StatisticsHomePage_ValidateFullReportStructureAndAvailability()
        {
            var fakePackageReport = "[{\"PackageId\":\"A\",\"Downloads\":1},{\"PackageId\":\"B\",\"Downloads\":2}]";
            var fakePackageVersionReport = "[{\"PackageId\":\"A\",\"PackageVersion\":\"1.0\",\"Downloads\":3},{\"PackageId\":\"A\",\"PackageVersion\":\"1.1\",\"Downloads\":4},{\"PackageId\":\"B\",\"PackageVersion\":\"1.0\",\"Downloads\":5}]";

            var fakeReportService = new Mock<IReportService>();

            fakeReportService.Setup(x => x.Load("RecentPopularity.json")).Returns(Task.FromResult(fakePackageReport));
            fakeReportService.Setup(x => x.Load("RecentPopularityDetail.json")).Returns(Task.FromResult(fakePackageVersionReport));

            var controller = new StatisticsController(new JsonStatisticsService(fakeReportService.Object), null);

            var model = (StatisticsPackagesViewModel)((ViewResult) await controller.Index()).Model;

            if (model.IsDownloadPackageAvailable)
            {
                foreach (var item in model.DownloadPackagesSummary)
                {
                    if (item.PackageId == "A" && item.Downloads == 1)
                    {
                        continue;
                    }
                    else if (item.PackageId == "B" && item.Downloads == 2)
                    {
                        continue;
                    }
                    throw new Exception("unexpected data in Package report");
                }
            }

            if (model.IsDownloadPackageDetailAvailable)
            {
                foreach (var item in model.DownloadPackageVersionsSummary)
                {
                    if (item.PackageId == "A" && item.PackageVersion == "1.0" & item.Downloads == 3)
                    {
                        continue;
                    }
                    if (item.PackageId == "A" && item.PackageVersion == "1.1" & item.Downloads == 4)
                    {
                        continue;
                    }
                    if (item.PackageId == "B" && item.PackageVersion == "1.0" & item.Downloads == 5)
                    {
                        continue;
                    }
                    throw new Exception("unexpected data in Package report");
                }
            }
        }

        [Fact]
        public async void StatisticsHomePage_Packages_ValidateReportStructureAndAvailability()
        {
            var fakePackageReport = "[{\"PackageId\":\"A\",\"Downloads\":42},{\"PackageId\":\"B\",\"Downloads\":64}]";

            var fakeReportService = new Mock<IReportService>();

            fakeReportService.Setup(x => x.Load("RecentPopularity.json")).Returns(Task.FromResult(fakePackageReport));

            var controller = new StatisticsController(new JsonStatisticsService(fakeReportService.Object), null);

            var model = (StatisticsPackagesViewModel)((ViewResult) await controller.Packages()).Model;

            int sum = 0;

            foreach (var item in model.DownloadPackagesAll)
            {
                sum += item.Downloads;
            }

            Assert.Equal<int>(106, sum);
        }

        [Fact]
        public async void StatisticsHomePage_PackageVersions_ValidateReportStructureAndAvailability()
        {
            var fakePackageVersionReport = "[{\"PackageId\":\"A\",\"PackageVersion\":\"1.0\",\"Downloads\":22},{\"PackageId\":\"A\",\"PackageVersion\":\"1.1\",\"Downloads\":20},{\"PackageId\":\"B\",\"PackageVersion\":\"1.0\",\"Downloads\":64}]";

            var fakeReportService = new Mock<IReportService>();

            fakeReportService.Setup(x => x.Load("RecentPopularityDetail.json")).Returns(Task.FromResult(fakePackageVersionReport));

            var controller = new StatisticsController(new JsonStatisticsService(fakeReportService.Object), null);

            var model = (StatisticsPackagesViewModel)((ViewResult) await controller.PackageVersions()).Model;

            int sum = 0;

            foreach (var item in model.DownloadPackageVersionsAll)
            {
                sum += item.Downloads;
            }

            Assert.Equal<int>(106, sum);
        }

        [Fact]
        public async void StatisticsHomePage_Per_Package_ValidateReportStructureAndAvailability()
        {
            string PackageId = "A";

            var fakeReport = "[{\"PackageVersion\":\"1.0\",\"Downloads\":101},{\"PackageVersion\":\"2.1\",\"Downloads\":202}]";

            var fakeReportService = new Mock<IReportService>();

            fakeReportService.Setup(x => x.Load("RecentPopularity_" + PackageId + ".json")).Returns(Task.FromResult(fakeReport));

            var controller = new StatisticsController(new JsonStatisticsService(fakeReportService.Object), null);

            var model = (StatisticsPackagesViewModel)((ViewResult) await controller.PackageDownloadsByVersion(PackageId)).Model;

            int sum = 0;

            foreach (var item in model.PackageDownloadsByVersion)
            {
                sum += item.Downloads;
            }

            Assert.Equal<int>(303, sum);
            Assert.Equal<int>(303, model.TotalPackageDownloads);
        }

        [Fact]
        public async void StatisticsHomePage_Packages_Negative_ValidateThrowOnInvalidStructure()
        {
            var fakePackageReport = "[{\"Lala\":\"A\",\"Downloads\":303}]";

            var fakeReportService = new Mock<IReportService>();

            fakeReportService.Setup(x => x.Load("RecentPopularity.json")).Returns(Task.FromResult(fakePackageReport));

            var controller = new StatisticsController(new JsonStatisticsService(fakeReportService.Object), null);

            bool hasException = false;

            try
            {
                var model = (StatisticsPackagesViewModel)((ViewResult) await controller.Packages()).Model;
                hasException = false;
            }
            catch (Exception)
            {
                //  we don't care too much about the exact type of the exception
                hasException = true;
            }

            if (!hasException)
            {
                throw new Exception("this exception thrown because expected exception was not thrown");
            }
        }

        public class TheStatsAction
        {
            [Fact]
            public void UseServerCultureIfLanguageHeadersIsMissing()
            {
                // Arrange
                var currentCulture = CultureInfo.CurrentCulture;

                try
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");

                    var statsService = new Mock<IAggregateStatsService>(MockBehavior.Strict);
                    var stats = new AggregateStats
                    {
                        Downloads = 2013,
                        TotalPackages = 1000,
                        UniquePackages = 500
                    };
                    statsService.Setup(s => s.GetAggregateStats()).Returns(stats);

                    var controller = CreateController(statsService);

                    // Act
                    var result = controller.Totals() as JsonResult;

                    // Asssert
                    Assert.NotNull(result);
                    dynamic data = result.Data;

                    Assert.Equal("2,013", (string)data.Downloads);
                    Assert.Equal("500", (string)data.UniquePackages);
                    Assert.Equal("1,000", (string)data.TotalPackages);
                }
                finally
                {
                    Thread.CurrentThread.CurrentCulture = currentCulture;
                }
            }

            [Fact]
            public void UseClientCultureIfLanguageHeadersIsPresent()
            {
                // Arrange
                var statsService = new Mock<IAggregateStatsService>(MockBehavior.Strict);
                var stats = new AggregateStats
                {
                    Downloads = 2013,
                    TotalPackages = 1000,
                    UniquePackages = 500
                };
                statsService.Setup(s => s.GetAggregateStats()).Returns(stats);

                var request = new Mock<HttpRequestBase>();
                request.Setup(r => r.UserLanguages).Returns(new string[] { "vi-VN" });

                var controller = CreateController(statsService, request);

                // Act
                var result = controller.Totals() as JsonResult;

                // Asssert
                Assert.NotNull(result);
                dynamic data = result.Data;

                Assert.Equal("2.013", (string)data.Downloads);
                Assert.Equal("500", (string)data.UniquePackages);
                Assert.Equal("1.000", (string)data.TotalPackages);
            }
        }

        public static StatisticsController CreateController(Mock<IAggregateStatsService> statsService, Mock<HttpRequestBase> request = null)
        {
            request = request ?? new Mock<HttpRequestBase>();

            var context = new Mock<HttpContextBase>();
            context.SetupGet(s => s.Request).Returns(request.Object);

            var controller = new StatisticsController(null, statsService.Object);
            controller.ControllerContext = new ControllerContext(context.Object, new RouteData(), controller);

            return controller;
        }
    }
}

