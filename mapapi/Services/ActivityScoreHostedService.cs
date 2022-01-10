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
    public class ActivityScoreHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<ActivityScoreHostedService> _logger;
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly GeosocdbContext dbcontext;
        private const float reviewCoeff = (float)0.23;
        private const float repostCoeff = (float)0.16;
        private const float favouriteCoeff = (float)0.15;
        private const float visitCoeff = (float)0.19;
        private const float profitValue = 6768;
        private DateTime prevDate = DateTime.UtcNow.AddHours(-24); //24
        private readonly Dictionary<string, int> EntityTypesDict = new Dictionary<string, int>
        {
            {"object", 1},
            {"place", 2}, 
            {"event", 3}, 
            {"route", 4}, 
        };

        public ActivityScoreHostedService(ILogger<ActivityScoreHostedService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            var scope = _scopeFactory.CreateScope();
            dbcontext = scope.ServiceProvider.GetRequiredService<GeosocdbContext>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            
            _logger.LogInformation("ActivityScore Hosted Service running.");

            _timer = new Timer(UpdateActivityScore, null, TimeSpan.Zero,
                TimeSpan.FromHours(24));

            return Task.CompletedTask;
        }

        private void UpdateActivityScore(object state)
        {
            //получаем все объекты, в которых совершались какие-либо действия в течение недели
            //типы действий: отзыв, репост, избранное, просмотры
            var @objects = dbcontext.Objects.Where(s =>
                dbcontext.Reviews.Any(e =>
                    e.TypeId == EntityTypesDict["object"] &&
                    DateTime.Compare(prevDate, e.PostedDate) < 0 &&
                    e.EntityId == s.IdEntity) ||
                dbcontext.Sharings.Any(e =>
                    e.TypeId == EntityTypesDict["object"] &&
                    DateTime.Compare(prevDate, e.ShareTime) < 0 &&
                    e.EntityId == s.IdEntity) ||
                dbcontext.Favourites.Any(e =>
                    e.TypeId == EntityTypesDict["object"] &&
                    DateTime.Compare(prevDate, e.AddedTime) < 0 &&
                    e.EntityId == s.IdEntity) ||
                dbcontext.ViewStatistics.Any(e =>
                    e.TypeId == EntityTypesDict["object"] &&
                    DateTime.Compare(prevDate, e.VisitedTime) < 0 &&
                    e.EntityId == s.IdEntity)
                ).ToList();
            List<int> usersIdArray = new List<int>();
            foreach (var @object in @objects)
            {
                _logger.LogInformation($"objectsId = {@object.IdEntity}");
                List<int> usersIdArrayTmp = UpdateUsersScore(@object.TypeId, @object.IdEntity);
                if (usersIdArrayTmp == null)
                    _logger.LogInformation($"Error wile updating data. ObjectId = {@object.IdEntity}");
                else
                {
                    usersIdArray = usersIdArray.Union(usersIdArrayTmp).ToList();
                }
            }

            var places = dbcontext.Places.Where(s =>
                dbcontext.Reviews.Any(e =>
                    e.TypeId == EntityTypesDict["place"] &&
                    DateTime.Compare(prevDate, e.PostedDate) < 0 &&
                    e.EntityId == s.IdEntity) ||
                dbcontext.Sharings.Any(e =>
                    e.TypeId == EntityTypesDict["place"] &&
                    DateTime.Compare(prevDate, e.ShareTime) < 0 &&
                    e.EntityId == s.IdEntity) ||
                dbcontext.Favourites.Any(e =>
                    e.TypeId == EntityTypesDict["place"] &&
                    DateTime.Compare(prevDate, e.AddedTime) < 0 &&
                    e.EntityId == s.IdEntity) ||
                dbcontext.ViewStatistics.Any(e =>
                    e.TypeId == EntityTypesDict["place"] &&
                    DateTime.Compare(prevDate, e.VisitedTime) < 0 &&
                    e.EntityId == s.IdEntity)
                ).ToList();

            foreach (var place in places)
            {
                List<int> usersIdArrayTmp = UpdateUsersScore(place.TypeId, place.IdEntity);
                if (usersIdArrayTmp == null)
                    _logger.LogInformation($"Error wile updating data. PlaceId = {place.IdEntity}");
                else
                {
                    usersIdArray = usersIdArray.Union(usersIdArrayTmp).ToList();
                }
            }

            var events = dbcontext.Events.Where(s =>
                dbcontext.Reviews.Any(e =>
                    e.TypeId == EntityTypesDict["event"] &&
                    DateTime.Compare(prevDate, e.PostedDate) < 0 &&
                    e.EntityId == s.IdEntity) ||
                dbcontext.Sharings.Any(e =>
                    e.TypeId == EntityTypesDict["event"] &&
                    DateTime.Compare(prevDate, e.ShareTime) < 0 &&
                    e.EntityId == s.IdEntity) ||
                dbcontext.Favourites.Any(e =>
                    e.TypeId == EntityTypesDict["event"] &&
                    DateTime.Compare(prevDate, e.AddedTime) < 0 &&
                    e.EntityId == s.IdEntity) ||
                dbcontext.ViewStatistics.Any(e =>
                    e.TypeId == EntityTypesDict["event"] &&
                    DateTime.Compare(prevDate, e.VisitedTime) < 0 &&
                    e.EntityId == s.IdEntity)
                ).ToList();

            foreach (var @event in events)
            {
                List<int> usersIdArrayTmp = UpdateUsersScore(@event.TypeId, @event.IdEntity);
                if (usersIdArrayTmp == null)
                    _logger.LogInformation($"Error wile updating data. PlaceId = {@event.IdEntity}");
                else
                {
                    usersIdArray = usersIdArray.Union(usersIdArrayTmp).ToList();
                }
            }

            var routes = dbcontext.Route.Where(s =>
               dbcontext.Reviews.Any(e =>
                   e.TypeId == EntityTypesDict["route"] &&
                   DateTime.Compare(prevDate, e.PostedDate) < 0 &&
                   e.EntityId == s.IdEntity) ||
               dbcontext.Sharings.Any(e =>
                   e.TypeId == EntityTypesDict["route"] &&
                   DateTime.Compare(prevDate, e.ShareTime) < 0 &&
                   e.EntityId == s.IdEntity) ||
               dbcontext.Favourites.Any(e =>
                   e.TypeId == EntityTypesDict["route"] &&
                   DateTime.Compare(prevDate, e.AddedTime) < 0 &&
                   e.EntityId == s.IdEntity) ||
               dbcontext.ViewStatistics.Any(e =>
                   e.TypeId == EntityTypesDict["route"] &&
                   DateTime.Compare(prevDate, e.VisitedTime) < 0 &&
                   e.EntityId == s.IdEntity)
               ).ToList();

            foreach (var route in routes)
            {
                List<int> usersIdArrayTmp = UpdateUsersScore(route.TypeId, route.IdEntity);
                if (usersIdArrayTmp == null)
                    _logger.LogInformation($"Error wile updating data. PlaceId = {route.IdEntity}");
                else
                {
                    usersIdArray = usersIdArray.Union(usersIdArrayTmp).ToList();
                }
            }
            string usersIdStr = "";
            foreach(int userId in usersIdArray)
            {
                usersIdStr += userId.ToString() + " ";
            }
            _logger.LogInformation(
                $"Activity score for {usersIdArray.Count()} users ( {usersIdStr}) updated.");
        }
        //функция для начисления коинов активности пользователям, проявлявшим активность 
        //в конкретной указанной сущности. typeId и entityId сущности указываются в параметрах.
        //Функция возвращает количество пользователей, коины для которых были обновлены
        //В случае неудачи сохранения данных в БД, функция вернет -1 и коины пользователей
        //сущности не сохранятся.
        private List<int> UpdateUsersScore(int typeId, long entityId) 
        {
            var visitorsId = dbcontext.ViewStatistics
                .Where(s => s.TypeId == typeId && s.EntityId == entityId && s.UserId != null)
                .Select(s => s.UserId)
                .Distinct()
                .Cast<int>()
                .ToList();
            //словарь для сохранения id пользователя и суммы баллов за действия этого пользователя
            Dictionary<int, float> activity_score = new Dictionary<int, float>(visitorsId.Count());
            foreach (int visitorId in visitorsId)
            {
                float visitor_score_sum = 0;
                var review = dbcontext.Reviews
                    .FirstOrDefault(
                        s => s.TypeId == typeId &&
                        s.EntityId == entityId &&
                        DateTime.Compare(prevDate, s.PostedDate) < 0 &&
                        s.UserId == visitorId);
                visitor_score_sum += (float)(review != null ? reviewCoeff : 0);
                var sharingsCount = dbcontext.Sharings
                    .Where(
                        s => s.TypeId == typeId &&
                        s.EntityId == entityId &&
                        DateTime.Compare(prevDate, s.ShareTime) < 0 &&
                        s.UserId == visitorId)
                    .Count();
                visitor_score_sum += sharingsCount * repostCoeff;
                var favourite = dbcontext.Favourites
                    .FirstOrDefault(
                        s => s.TypeId == typeId &&
                        s.EntityId == entityId &&
                        DateTime.Compare(prevDate, s.AddedTime) < 0 &&
                        s.UserId == visitorId);
                visitor_score_sum += (float)(favourite != null ? favouriteCoeff : 0);
                var visitsCount = dbcontext.ViewStatistics
                    .Where(
                        s => s.TypeId == typeId &&
                        s.EntityId == entityId &&
                        DateTime.Compare(prevDate, s.VisitedTime) < 0 &&
                        s.UserId == visitorId)
                    .Count();
                visitor_score_sum += visitsCount * visitCoeff;
                activity_score.Add(visitorId, visitor_score_sum);
            }
            float object_score_sum = activity_score.Values.Sum();
            var entityVisitsCount = dbcontext.ViewStatistics
                    .Where(
                        s => s.TypeId == typeId &&
                        s.EntityId == entityId &&
                        DateTime.Compare(prevDate, s.VisitedTime) < 0)
                    .Count();
            foreach (int visitorId in visitorsId)
            {
                //удельный вес набранных баллов 
                float coin_wight = activity_score[visitorId] / object_score_sum * 100;
                //начисляемые коины с учетом распределения
                float activity_coin = coin_wight * activity_score[visitorId] * entityVisitsCount / profitValue * (float)0.3;
                var visitor = dbcontext.Users.First(s => s.IdUser == visitorId);
                visitor.ActivityCoin += activity_coin;
            }
            try
            {
                dbcontext.SaveChanges();
            }
            catch(Exception e)
            {
                return null;
            }
            return visitorsId;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ActivityScore Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
