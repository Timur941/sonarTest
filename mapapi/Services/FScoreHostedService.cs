using mapapi.Models.SocialAction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace mapapi.Services
{
    public class FScoreHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<FScoreHostedService> _logger;
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly GeosocdbContext dbcontext;
        private const float reviewCoeff = 17;
        private const float repostCoeff = 8;
        private const float favouriteCoeff = 10;
        private const float visitCoeff = (float)1.5;
        private readonly Dictionary<int, float> TimeCoefficient = new Dictionary<int, float>
        {
            {365, 2}, //коэффициент для года
            {91, 3}, //коэффициент для квартала
            {31, 7}, //коэффициент для месяца
            {7, 12}, //коэффициент для недели
            {1, 20} //коэффициент для дня
        };

        public FScoreHostedService(ILogger<FScoreHostedService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            var scope = _scopeFactory.CreateScope();
            dbcontext = scope.ServiceProvider.GetRequiredService<GeosocdbContext>();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("FScore Hosted Service running.");

            _timer = new Timer(UpdateFScore, null, TimeSpan.Zero,
                TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }
        private void UpdateFScore(object state)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            //Подсчет для объектов
            var @objects = dbcontext.Objects.ToList();
            foreach (var @object in objects)
            {
                float ntReview = 0, ntRepost = 0, ntVisit = 0, ntFavourite = 0;
                //подсчет суммы nt за временные промежутки, за исключением всего времени
                for(int i = 0; i < 4; i++)
                {
                    ntReview += CalcNtReview(@object.TypeId, @object.IdEntity, TimeCoefficient.ElementAt(i).Key, 
                        TimeCoefficient.ElementAt(i+1).Key, TimeCoefficient.ElementAt(i).Value);
                    ntRepost += CalcNtRepost(@object.TypeId, @object.IdEntity, TimeCoefficient.ElementAt(i).Key,
                        TimeCoefficient.ElementAt(i + 1).Key, TimeCoefficient.ElementAt(i).Value);
                    ntFavourite += CalcNtFavourite(@object.TypeId, @object.IdEntity, TimeCoefficient.ElementAt(i).Key,
                        TimeCoefficient.ElementAt(i + 1).Key, TimeCoefficient.ElementAt(i).Value);
                    ntVisit += CalcNtVisit(@object.TypeId, @object.IdEntity, TimeCoefficient.ElementAt(i).Key,
                        TimeCoefficient.ElementAt(i + 1).Key, TimeCoefficient.ElementAt(i).Value);
                }
                ntReview += CalcNtReview(@object.TypeId, @object.IdEntity, TimeCoefficient.ElementAt(4).Key,
                        0, TimeCoefficient.ElementAt(4).Value);
                ntRepost += CalcNtRepost(@object.TypeId, @object.IdEntity, TimeCoefficient.ElementAt(4).Key,
                        0, TimeCoefficient.ElementAt(4).Value);
                ntFavourite += CalcNtFavourite(@object.TypeId, @object.IdEntity, TimeCoefficient.ElementAt(4).Key,
                        0, TimeCoefficient.ElementAt(4).Value);
                ntVisit += CalcNtVisit(@object.TypeId, @object.IdEntity, TimeCoefficient.ElementAt(4).Key,
                        0, TimeCoefficient.ElementAt(4).Value);
                //подсчет суммы nt с учетом всео времени 
                ntReview += CalcNtReview(@object.TypeId, @object.IdEntity, 0, 0, 1);
                ntRepost += CalcNtRepost(@object.TypeId, @object.IdEntity, 0, 0, 1);
                ntFavourite += CalcNtFavourite(@object.TypeId, @object.IdEntity, 0, 0, 1);
                ntVisit += CalcNtVisit(@object.TypeId, @object.IdEntity, 0, 0, 1);
                var coeffSum = TimeCoefficient.Values.Sum() + 1; // 1 добавляется с учетом всего времени

                ntReview /= coeffSum; 
                ntRepost /= coeffSum;
                ntFavourite /= coeffSum;
                ntVisit /= coeffSum;

                float f_score = (float)Math.Log10(1 + ntReview)*reviewCoeff + 
                    (float)Math.Log10(1 + ntRepost)*repostCoeff +
                    (float)Math.Log10(1 + ntFavourite)*favouriteCoeff +
                    (float)Math.Log10(1 + ntVisit)*visitCoeff;

                @object.FScore = f_score;     
            }
            //Подсчет для мест
            var places = dbcontext.Places.ToList();
            foreach (var place in places)
            {
                float ntReview = 0, ntRepost = 0, ntVisit = 0, ntFavourite = 0;
                //подсчет суммы nt за временные промежутки, за исключением всего времени
                for (int i = 0; i < 4; i++)
                {
                    ntReview += CalcNtReview(place.TypeId, place.IdEntity, TimeCoefficient.ElementAt(i).Key,
                        TimeCoefficient.ElementAt(i + 1).Key, TimeCoefficient.ElementAt(i).Value);
                    ntRepost += CalcNtRepost(place.TypeId, place.IdEntity, TimeCoefficient.ElementAt(i).Key,
                        TimeCoefficient.ElementAt(i + 1).Key, TimeCoefficient.ElementAt(i).Value);
                    ntFavourite += CalcNtFavourite(place.TypeId, place.IdEntity, TimeCoefficient.ElementAt(i).Key,
                        TimeCoefficient.ElementAt(i + 1).Key, TimeCoefficient.ElementAt(i).Value);
                    ntVisit += CalcNtVisit(place.TypeId, place.IdEntity, TimeCoefficient.ElementAt(i).Key,
                        TimeCoefficient.ElementAt(i + 1).Key, TimeCoefficient.ElementAt(i).Value);
                }
                ntReview += CalcNtReview(place.TypeId, place.IdEntity, TimeCoefficient.ElementAt(4).Key,
                        0, TimeCoefficient.ElementAt(4).Value);
                ntRepost += CalcNtRepost(place.TypeId, place.IdEntity, TimeCoefficient.ElementAt(4).Key,
                        0, TimeCoefficient.ElementAt(4).Value);
                ntFavourite += CalcNtFavourite(place.TypeId, place.IdEntity, TimeCoefficient.ElementAt(4).Key,
                        0, TimeCoefficient.ElementAt(4).Value);
                ntVisit += CalcNtVisit(place.TypeId, place.IdEntity, TimeCoefficient.ElementAt(4).Key,
                        0, TimeCoefficient.ElementAt(4).Value);
                //подсчет суммы nt с учетом всео времени 
                ntReview += CalcNtReview(place.TypeId, place.IdEntity, 0, 0, 1);
                ntRepost += CalcNtRepost(place.TypeId, place.IdEntity, 0, 0, 1);
                ntFavourite += CalcNtFavourite(place.TypeId, place.IdEntity, 0, 0, 1);
                ntVisit += CalcNtVisit(place.TypeId, place.IdEntity, 0, 0, 1);
                var coeffSum = TimeCoefficient.Values.Sum() + 1; // 1 добавляется с учетом всего времени

                ntReview /= coeffSum;
                ntRepost /= coeffSum;
                ntFavourite /= coeffSum;
                ntVisit /= coeffSum;

                float f_score = (float)Math.Log10(1 + ntReview) * reviewCoeff +
                    (float)Math.Log10(1 + ntRepost) * repostCoeff +
                    (float)Math.Log10(1 + ntFavourite) * favouriteCoeff +
                    (float)Math.Log10(1 + ntVisit) * visitCoeff;

                place.FScore = f_score;
            }
            //Подсчет для событий
            var events = dbcontext.Events.ToList();
            foreach (var @event in events)
            {
                float ntReview = 0, ntRepost = 0, ntVisit = 0, ntFavourite = 0;
                //подсчет суммы nt за временные промежутки, за исключением всего времени
                for (int i = 0; i < 4; i++)
                {
                    ntReview += CalcNtReview(@event.TypeId, @event.IdEntity, TimeCoefficient.ElementAt(i).Key,
                        TimeCoefficient.ElementAt(i + 1).Key, TimeCoefficient.ElementAt(i).Value);
                    ntRepost += CalcNtRepost(@event.TypeId, @event.IdEntity, TimeCoefficient.ElementAt(i).Key,
                        TimeCoefficient.ElementAt(i + 1).Key, TimeCoefficient.ElementAt(i).Value);
                    ntFavourite += CalcNtFavourite(@event.TypeId, @event.IdEntity, TimeCoefficient.ElementAt(i).Key,
                        TimeCoefficient.ElementAt(i + 1).Key, TimeCoefficient.ElementAt(i).Value);
                    ntVisit += CalcNtVisit(@event.TypeId, @event.IdEntity, TimeCoefficient.ElementAt(i).Key,
                        TimeCoefficient.ElementAt(i + 1).Key, TimeCoefficient.ElementAt(i).Value);
                }
                ntReview += CalcNtReview(@event.TypeId, @event.IdEntity, TimeCoefficient.ElementAt(4).Key,
                        0, TimeCoefficient.ElementAt(4).Value);
                ntRepost += CalcNtRepost(@event.TypeId, @event.IdEntity, TimeCoefficient.ElementAt(4).Key,
                        0, TimeCoefficient.ElementAt(4).Value);
                ntFavourite += CalcNtFavourite(@event.TypeId, @event.IdEntity, TimeCoefficient.ElementAt(4).Key,
                        0, TimeCoefficient.ElementAt(4).Value);
                ntVisit += CalcNtVisit(@event.TypeId, @event.IdEntity, TimeCoefficient.ElementAt(4).Key,
                        0, TimeCoefficient.ElementAt(4).Value);
                //подсчет суммы nt с учетом всео времени 
                ntReview += CalcNtReview(@event.TypeId, @event.IdEntity, 0, 0, 1);
                ntRepost += CalcNtRepost(@event.TypeId, @event.IdEntity, 0, 0, 1);
                ntFavourite += CalcNtFavourite(@event.TypeId, @event.IdEntity, 0, 0, 1);
                ntVisit += CalcNtVisit(@event.TypeId, @event.IdEntity, 0, 0, 1);
                var coeffSum = TimeCoefficient.Values.Sum() + 1; // 1 добавляется с учетом всего времени

                ntReview /= coeffSum;
                ntRepost /= coeffSum;
                ntFavourite /= coeffSum;
                ntVisit /= coeffSum;

                float f_score = (float)Math.Log10(1 + ntReview) * reviewCoeff +
                    (float)Math.Log10(1 + ntRepost) * repostCoeff +
                    (float)Math.Log10(1 + ntFavourite) * favouriteCoeff +
                    (float)Math.Log10(1 + ntVisit) * visitCoeff;

                @event.FScore = f_score;
            }
            //Подсчет для маршрутов
            var routes = dbcontext.Route.ToList();
            foreach (var route in routes)
            {
                float ntReview = 0, ntRepost = 0, ntVisit = 0, ntFavourite = 0;
                //подсчет суммы nt за временные промежутки, за исключением всего времени
                for (int i = 0; i < 4; i++)
                {
                    ntReview += CalcNtReview(route.TypeId, route.IdEntity, TimeCoefficient.ElementAt(i).Key,
                        TimeCoefficient.ElementAt(i + 1).Key, TimeCoefficient.ElementAt(i).Value);
                    ntRepost += CalcNtRepost(route.TypeId, route.IdEntity, TimeCoefficient.ElementAt(i).Key,
                        TimeCoefficient.ElementAt(i + 1).Key, TimeCoefficient.ElementAt(i).Value);
                    ntFavourite += CalcNtFavourite(route.TypeId, route.IdEntity, TimeCoefficient.ElementAt(i).Key,
                        TimeCoefficient.ElementAt(i + 1).Key, TimeCoefficient.ElementAt(i).Value);
                    ntVisit += CalcNtVisit(route.TypeId, route.IdEntity, TimeCoefficient.ElementAt(i).Key,
                        TimeCoefficient.ElementAt(i + 1).Key, TimeCoefficient.ElementAt(i).Value);
                }
                ntReview += CalcNtReview(route.TypeId, route.IdEntity, TimeCoefficient.ElementAt(4).Key,
                        0, TimeCoefficient.ElementAt(4).Value);
                ntRepost += CalcNtRepost(route.TypeId, route.IdEntity, TimeCoefficient.ElementAt(4).Key,
                        0, TimeCoefficient.ElementAt(4).Value);
                ntFavourite += CalcNtFavourite(route.TypeId, route.IdEntity, TimeCoefficient.ElementAt(4).Key,
                        0, TimeCoefficient.ElementAt(4).Value);
                ntVisit += CalcNtVisit(route.TypeId, route.IdEntity, TimeCoefficient.ElementAt(4).Key,
                        0, TimeCoefficient.ElementAt(4).Value);
                //подсчет суммы nt с учетом всео времени 
                ntReview += CalcNtReview(route.TypeId, route.IdEntity, 0, 0, 1);
                ntRepost += CalcNtRepost(route.TypeId, route.IdEntity, 0, 0, 1);
                ntFavourite += CalcNtFavourite(route.TypeId, route.IdEntity, 0, 0, 1);
                ntVisit += CalcNtVisit(route.TypeId, route.IdEntity, 0, 0, 1);
                var coeffSum = TimeCoefficient.Values.Sum() + 1; // 1 добавляется с учетом всего времени

                ntReview /= coeffSum;
                ntRepost /= coeffSum;
                ntFavourite /= coeffSum;
                ntVisit /= coeffSum;

                float f_score = (float)Math.Log10(1 + ntReview) * reviewCoeff +
                    (float)Math.Log10(1 + ntRepost) * repostCoeff +
                    (float)Math.Log10(1 + ntFavourite) * favouriteCoeff +
                    (float)Math.Log10(1 + ntVisit) * visitCoeff;

                route.FScore = f_score;
            }
            dbcontext.SaveChanges();
            stopWatch.Stop();
            //Подсчет времени необязателен, используется для мониторинга 
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);

            _logger.LogInformation(
                $"FSCore for {objects.Count()} objects, {places.Count()} places, {events.Count()} events, {routes.Count()} routes updated. Spent time: {elapsedTime}");
        }

        private float CalcNtReview(int typeId, long entityId, int daysBefore, int daysAfter, float coeff)
        {
            int reviewsCount;
            if(daysBefore == 0)
            {
                reviewsCount = dbcontext.Reviews
                    .Where(i =>
                        DateTime.Compare(i.PostedDate, DateTime.UtcNow.AddDays(-365)) < 0 &&
                        i.TypeId == typeId &&
                        i.EntityId == entityId)
                    .Count();
            }
            else
            {
                var beforeDate = DateTime.UtcNow.AddDays(-daysBefore);
                var afterDate = DateTime.UtcNow.AddDays(-daysAfter);
                reviewsCount = dbcontext.Reviews
                    .Where(i =>
                        DateTime.Compare(beforeDate, i.PostedDate) < 0 &&
                        DateTime.Compare(afterDate, i.PostedDate) > 0 &&
                        i.TypeId == typeId &&
                        i.EntityId == entityId)
                    .Count();
            }
            var ntReview = reviewsCount * coeff;
            return ntReview;
        }
        private float CalcNtRepost(int typeId, long entityId, int daysBefore, int daysAfter, float coeff)
        {
            int repostsCount;
            if (daysBefore == 0)
            {
                repostsCount = dbcontext.Sharings
                    .Where(i =>
                        DateTime.Compare(i.ShareTime, DateTime.UtcNow.AddDays(-365)) < 0 &&
                        i.TypeId == typeId &&
                        i.EntityId == entityId)
                    .Count();
            }
            else
            {
                var beforeDate = DateTime.UtcNow.AddDays(-daysBefore);
                var afterDate = DateTime.UtcNow.AddDays(-daysAfter);
                repostsCount = dbcontext.Sharings
                    .Where(i =>
                        DateTime.Compare(beforeDate, i.ShareTime) < 0 &&
                        DateTime.Compare(afterDate, i.ShareTime) > 0 &&
                        i.TypeId == typeId &&
                        i.EntityId == entityId)
                    .Count();
            }
            var ntRepost = repostsCount * coeff;
            return ntRepost;
        }
        private float CalcNtFavourite(int typeId, long entityId, int daysBefore, int daysAfter, float coeff)
        {
            int favouritesCount;
            if (daysBefore == 0)
            {
                favouritesCount = dbcontext.Favourites
                    .Where(i =>
                        DateTime.Compare(i.AddedTime, DateTime.UtcNow.AddDays(-365)) < 0 &&
                        i.TypeId == typeId &&
                        i.EntityId == entityId)
                    .Count();
            }
            else
            {
                var beforeDate = DateTime.UtcNow.AddDays(-daysBefore);
                var afterDate = DateTime.UtcNow.AddDays(-daysAfter);
                favouritesCount = dbcontext.Favourites
                    .Where(i =>
                        DateTime.Compare(beforeDate, i.AddedTime) < 0 &&
                        DateTime.Compare(afterDate, i.AddedTime) > 0 &&
                        i.TypeId == typeId &&
                        i.EntityId == entityId)
                    .Count();
            }
            var ntFavourite = favouritesCount * coeff;
            return ntFavourite;
        }
        private float CalcNtVisit(int typeId, long entityId, int daysBefore, int daysAfter, float coeff)
        {
            int visitsCount;
            if (daysBefore == 0)
            {
                visitsCount = dbcontext.ViewStatistics
                    .Where(i =>
                        DateTime.Compare(i.VisitedTime, DateTime.UtcNow.AddDays(-365)) < 0 &&
                        i.TypeId == typeId &&
                        i.EntityId == entityId)
                    .Count();
            }
            else
            {
                var beforeDate = DateTime.UtcNow.AddDays(-daysBefore);
                var afterDate = DateTime.UtcNow.AddDays(-daysAfter);
                visitsCount = dbcontext.ViewStatistics
                    .Where(i =>
                        DateTime.Compare(beforeDate, i.VisitedTime) < 0 &&
                        DateTime.Compare(afterDate, i.VisitedTime) > 0 &&
                        i.TypeId == typeId &&
                        i.EntityId == entityId)
                    .Count();
            }
            var ntVisit = visitsCount * coeff;
            return ntVisit;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("FScore Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
