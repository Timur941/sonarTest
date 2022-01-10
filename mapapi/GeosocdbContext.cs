using System;
using mapapi.Models;
using mapapi.Models.SocialAction;
using mapapi.Models.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace mapapi
{
    public partial class GeosocdbContext : DbContext
    {
        public GeosocdbContext()
        {
        }

        public GeosocdbContext(DbContextOptions<GeosocdbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<Object> Objects { get; set; }
        public virtual DbSet<Place> Places { get; set; }
        public virtual DbSet<Type> Types { get; set; }
        public virtual DbSet<Photo> Photos { get; set; }
        public virtual DbSet<Route> Route { get; set; }
        public virtual DbSet<RoutePoint> RoutePoint { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Chat> ChatRooms { get; set; }
        public virtual DbSet<ChatUser> ChatUsers { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<ChatConnection> ChatConnections { get; set; }
        public virtual DbSet<CategoryClassifier> CategoryClassifiers { get; set; }
        public virtual DbSet<ObjectSchedule> ObjectScheduleRows { get; set; }
        public virtual DbSet<LikeDislike> LikeDislikes { get; set; }
        public virtual DbSet<Review> Reviews { get; set; }
        public virtual DbSet<ViewStatistics> ViewStatistics { get; set; }
        public virtual DbSet<Favourite> Favourites { get; set; }
        public virtual DbSet<UprofileImg> UprofileImgs { get; set; }
        public virtual DbSet<Sharing> Sharings { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<Ucomment> Ucomments { get; set; }
        public virtual DbSet<Friendship> Friendships { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("postgis")
                .HasAnnotation("Relational:Collation", "Russian_Russia.1251");

            modelBuilder.Entity<City>(entity =>
            {
                entity.HasKey(e => e.IdCity)
                    .HasName("city_pkey");

                entity.ToTable("city");

                entity.Property(e => e.IdCity).HasColumnName("id_city");
                entity.Property(e => e.Name)
                    .HasColumnType("character varying")
                    .HasColumnName("name");
                entity.Property(e => e.UrlName)
                    .HasColumnType("character varying")
                    .HasColumnName("urlname");
                entity.Property(e => e.Way).HasColumnName("way");

                //entity.HasOne(d => d.CategoryClassifier);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.IdCategory)
                    .HasName("category_pkey");

                entity.ToTable("category");

                entity.Property(e => e.IdCategory).HasColumnName("id_category");
                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasColumnType("character varying")
                    .HasColumnName("category_name");
                entity.Property(e => e.TypeId).HasColumnName("type_id");
                entity.Property(e => e.CategoryClassifierId).HasColumnName("classifier");

                //entity.HasOne(d => d.CategoryClassifier);
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.IdEntity)
                    .HasName("event_pkey");

                entity.ToTable("event");

                entity.Property(e => e.IdEntity).HasColumnName("id_entity");
                entity.Property(e => e.AssociatedEntityType).HasColumnName("associated_entity_type");
                entity.Property(e => e.AssociatedEntityId).HasColumnName("associated_entity_id");
                entity.Property(e => e.CategoryId).HasColumnName("category_id");
                entity.Property(e => e.Date).HasColumnName("date");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Duration)
                    .HasColumnType("interval")
                    .HasColumnName("duration");
                entity.Property(e => e.PreviewDescription)
                    .HasColumnType("character varying")
                    .HasColumnName("preview_description");
                entity.Property(e => e.Price)
                    .HasColumnType("money")
                    .HasColumnName("price");
                entity.Property(e => e.Private).HasColumnName("private");
                entity.Property(e => e.Rating).HasColumnName("rating");
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnType("character varying")
                    .HasColumnName("title");
                entity.Property(e => e.PhotoId).HasColumnName("photo_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.TypeId).HasColumnName("type_id");
                entity.Property(e => e.Way).HasColumnName("way");
                entity.Property(e => e.FScore)
                    .HasColumnType("real")
                    .HasColumnName("f_score");
                entity.HasOne(d => d.Category);
                entity.Property(e => e.Phone)
                    .HasColumnType("character varying")
                    .HasColumnName("phone");
                entity.Property(e => e.Website)
                    .HasColumnType("character varying")
                    .HasColumnName("website");
                entity.Property(e => e.Vk)
                    .HasColumnType("character varying")
                    .HasColumnName("vk");
                entity.Property(e => e.Instagram)
                    .HasColumnType("character varying")
                    .HasColumnName("instagram");

                //entity.HasOne(d => d.Category)
                //    .WithMany(p => p.Events)
                //    .HasForeignKey(d => d.CategoryId)
                //    .HasConstraintName("category_id_FK");
            });

            modelBuilder.Entity<Object>(entity =>
            {
                entity.HasKey(e => e.IdEntity)
                    .HasName("object_pkey");

                entity.ToTable("object");

                entity.Property(e => e.IdEntity).HasColumnName("id_entity");
                entity.Property(e => e.Address)
                    .HasColumnType("character varying")
                    .HasColumnName("address");
                entity.Property(e => e.AgeLimit)
                    .HasMaxLength(3)
                    .HasColumnName("age_limit")
                    .IsFixedLength(true);
                entity.Property(e => e.CategoryId).HasColumnName("category_id");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.PreviewDescription)
                    .HasColumnType("character varying")
                    .HasColumnName("preview_description");
                entity.Property(e => e.Price)
                    .HasColumnType("money")
                    .HasColumnName("price");
                entity.Property(e => e.Private).HasColumnName("private");
                entity.Property(e => e.Rating).HasColumnName("rating");
                entity.Property(e => e.Title)
                    .HasColumnType("character varying")
                    .HasColumnName("title");
                entity.Property(e => e.TypeId).HasColumnName("type_id");
                entity.Property(e => e.Way).HasColumnName("way");
                //entity.HasOne(d => d.Category);
                entity.Property(e => e.PhotoId).HasColumnName("photo_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.FScore)
                    .HasColumnType("real")
                    .HasColumnName("f_score");
                entity.Property(e => e.Phone)
                    .HasColumnType("character varying")
                    .HasColumnName("phone");
                entity.Property(e => e.Website)
                    .HasColumnType("character varying")
                    .HasColumnName("website");
                entity.Property(e => e.Vk)
                    .HasColumnType("character varying")
                    .HasColumnName("vk");
                entity.Property(e => e.Instagram)
                    .HasColumnType("character varying")
                    .HasColumnName("instagram");

                //entity.HasOne(d => d.Photo);

                //entity.HasOne(d => d.Category)
                //    .WithMany(p => p.Objects)
                //    .HasForeignKey(d => d.CategoryId)
                //    .HasConstraintName("category_id_FK");
            });

            modelBuilder.Entity<Place>(entity =>
            {
                entity.HasKey(e => e.IdEntity)
                    .HasName("place_pkey");

                entity.ToTable("place");

                entity.Property(e => e.IdEntity).HasColumnName("id_entity");
                entity.Property(e => e.CategoryId).HasColumnName("category_id");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.PreviewDescription)
                    .HasColumnType("character varying")
                    .HasColumnName("preview_description");
                entity.Property(e => e.Private).HasColumnName("private");
                entity.Property(e => e.Address).HasColumnName("address")
                    .HasColumnType("character varying");
                entity.Property(e => e.Rating).HasColumnName("rating");
                entity.Property(e => e.Title)
                    .HasColumnType("character varying")
                    .HasColumnName("title");
                entity.Property(e => e.TypeId).HasColumnName("type_id");
                entity.Property(e => e.Way).HasColumnName("way");
                entity.HasOne(e => e.Category);
                entity.Property(e => e.PhotoId).HasColumnName("photo_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.FScore)
                    .HasColumnType("real")
                    .HasColumnName("f_score");
                entity.Property(e => e.Phone)
                    .HasColumnType("character varying")
                    .HasColumnName("phone");
                entity.Property(e => e.Website)
                    .HasColumnType("character varying")
                    .HasColumnName("website");
                entity.Property(e => e.Vk)
                    .HasColumnType("character varying")
                    .HasColumnName("vk");
                entity.Property(e => e.Instagram)
                    .HasColumnType("character varying")
                    .HasColumnName("instagram");


                //entity.HasOne(d => d.Category)
                //    .WithMany(p => p.Places)
                //    .HasForeignKey(d => d.CategoryId)
                //    .HasConstraintName("category_id_FK");
            });

            modelBuilder.Entity<Route>(entity =>
            {
                entity.HasKey(e => e.IdEntity)
                    .HasName("route_pkey");

                entity.ToTable("route");
                entity.Property(e => e.IdEntity).HasColumnName("id_entity");
                entity.Property(e => e.Way).HasColumnName("way");
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnType("character varying")
                    .HasColumnName("title");
                entity.Property(e => e.PreviewDescription)
                    .HasColumnType("character varying")
                    .HasColumnName("preview_description");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.CategoryId).HasColumnName("category_id");
                entity.Property(e => e.Rating).HasColumnName("rating");
                entity.Property(e => e.Private).HasColumnName("private");
                entity.Property(e => e.Duration)
                    .HasColumnType("time without time zone")
                    .HasColumnName("duration");
                entity.Property(e => e.Price)
                    .HasColumnType("money")
                    .HasColumnName("price");
                entity.Property(e => e.AgeLimit)
                    .HasMaxLength(3)
                    .HasColumnName("age_limit")
                    .IsFixedLength(true);
                entity.Property(e => e.Distance).HasColumnName("distance");
                entity.Property(e => e.TypeId).HasColumnName("type_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.FScore)
                    .HasColumnType("real")
                    .HasColumnName("f_score");
                entity.HasMany(d => d.RoutePoints);

                //entity.HasOne(d => d.Category)
                //    .WithMany(p => p.Events)
                //    .HasForeignKey(d => d.CategoryId)
                //    .HasConstraintName("category_id_FK");
            });

            modelBuilder.Entity<Type>(entity =>
            {
                entity.HasKey(e => e.IdType)
                    .HasName("type_pkey");

                entity.ToTable("type");

                entity.Property(e => e.IdType).HasColumnName("id_type");
                entity.Property(e => e.TypeName)
                    .IsRequired()
                    .HasColumnType("character varying")
                    .HasColumnName("type_name");
            });

            modelBuilder.Entity<RoutePoint>(entity =>
            {
                entity.HasKey(e => e.IdRoutePoint)
                    .HasName("route_point_pkey");

                entity.ToTable("route_point");

                entity.Property(e => e.IdRoutePoint).HasColumnName("id_route_point");
                entity.Property(e => e.RouteId).HasColumnName("route_id");
                entity.Property(e => e.Way).HasColumnName("way");
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnType("character varying")
                    .HasColumnName("title");
                entity.Property(e => e.Description).HasColumnName("description");

                //entity.HasOne(d => d.IdRoutePoint)
                //    .WithMany(p => p.RoutePoints)
                //    .HasForeignKey(d => d.RouteId)
                //    .HasConstraintName("route_id_FK");
            });

            modelBuilder.Entity<Photo>(entity =>
            {
                entity.HasKey(e => e.IdPhoto)
                    .HasName("photo_pkey");

                entity.ToTable("photo");

                entity.Property(e => e.IdPhoto).HasColumnName("id_photo");
                entity.Property(e => e.TypeId).HasColumnName("type_id");
                entity.Property(e => e.EntityId).HasColumnName("entity_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.OrigImgData)
                        .HasColumnType("bytea")
                        .HasColumnName("orig_img_data");
                entity.Property(e => e.ThumbnailImgData)
                        .HasColumnType("bytea")
                        .HasColumnName("thumbnail_img_data");
                entity.Property(e => e.Title)
                        .HasColumnType("character vatying")
                        .HasColumnName("title");
                entity.Property(e => e.PostedDate)
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("posted_date");
                entity.Property(e => e.Description)
                        .HasColumnType("character vatying")
                        .HasColumnName("description");
                entity.Property(e => e.ContentType)
                        .HasColumnType("character vatying")
                        .HasColumnName("content_type");
                entity.Property(e => e.FScore)
                    .HasColumnType("real")
                    .HasColumnName("f_score");
            });

            modelBuilder.Entity<UprofileImg>(entity =>
            {
                entity.HasKey(e => e.IdUprofileImg)
                    .HasName("uprofile_img_pkey");

                entity.ToTable("uprofile_img");

                entity.Property(e => e.IdUprofileImg).HasColumnName("id_uprofile_img");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.IsCurrent).HasColumnName("is_current");
                entity.Property(e => e.OrigImgData)
                        .HasColumnType("bytea")
                        .HasColumnName("orig_img_data");
                entity.Property(e => e.ThumbnailImgData)
                        .HasColumnType("bytea")
                        .HasColumnName("thumbnail_img_data");
                entity.Property(e => e.UploadedDate)
                        .HasColumnType("timestamp with zone")
                        .HasColumnName("uploaded_date");
                entity.Property(e => e.Description)
                        .HasColumnType("character varying")
                        .HasColumnName("description");
                entity.Property(e => e.ContentType)
                        .HasColumnType("character vatying")
                        .HasColumnName("content_type");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.IdUser)
                    .HasName("user_pkey");

                entity.ToTable("user");

                entity.Property(e => e.IdUser).HasColumnName("id_user");
                entity.Property(e => e.Username)
                        .HasColumnType("character varying")
                        .HasColumnName("username");
                entity.Property(e => e.Email)
                        .HasColumnType("character varying")
                        .HasColumnName("email");
                entity.Property(e => e.Password)
                        .HasColumnType("bytea")
                        .HasColumnName("password");
                entity.Property(e => e.Salt)
                        .HasColumnType("bytea")
                        .HasColumnName("salt");
                entity.Property(e => e.Role)
                        .HasColumnType("character varying")
                        .HasColumnName("role");
                entity.Property(e => e.ActivityCoin)
                        .HasColumnType("real")
                        .HasColumnName("activity_coin");
                entity.Property(e => e.Fullname)
                        .HasColumnType("character varying")
                        .HasColumnName("fullname");
                entity.Property(e => e.CreatedDate)
                        .HasColumnType("timestamptz")
                        .HasColumnName("created_date");
                entity.Property(e => e.BirthDate)
                        .HasColumnType("date")
                        .HasColumnName("birth_date");
            });

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.HasKey(e => e.IdChat)
                    .HasName("chat_pkey");

                entity.ToTable("chat");

                entity.Property(e => e.IdChat)
                    .HasColumnType("uuid")
                    .HasColumnName("id_chat");

                entity.Property(e => e.CreationTime)
                    .IsRequired()
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("creation_time");
                entity.Property(e => e.ChatName)
                    .HasColumnName("chat_name")
                    .HasColumnType("character varying");
                entity.Property(e => e.TypeId).HasColumnName("type_id");
                entity.Property(e => e.EntityId).HasColumnName("entity_id");
                entity.Property(e => e.Owner).HasColumnName("owner");
                entity.Property(e => e.Personal)
                    .HasColumnType("bool")
                    .HasColumnName("personal");
            });

            modelBuilder.Entity<ChatUser>(entity =>
            {
                entity.HasKey(e => e.IdChatUser)
                    .HasName("chat_user_pkey");

                entity.ToTable("chat_user");

                entity.Property(e => e.IdChatUser).HasColumnName("id_chat_user");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.ChatId)
                    .HasColumnType("uuid")
                    .HasColumnName("chat_id");
                //entity.HasKey(e => new { e.UserId, e.ChatId });
            });

            //modelBuilder.Entity<ChatUser>()
            //    .HasOne<Grade>(s => s.Grade)
            //    .WithMany(g => g.Students)
            //    .HasForeignKey(s => s.CurrentGradeId);

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.IdMessage)
                    .HasName("message_pkey");

                entity.ToTable("message");

                entity.Property(e => e.IdMessage).HasColumnName("id_message");
                entity.Property(e => e.ChatId)
                    .HasColumnType("uuid")
                    .HasColumnName("chat_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.MessageText)
                    .HasColumnName("message_text")
                    .HasColumnType("character varying");
                entity.Property(e => e.TypeId).HasColumnName("type_id");
                entity.Property(e => e.EntityId).HasColumnName("entity_id");
                entity.Property(e => e.Replied).HasColumnName("replied");
                entity.Property(e => e.IsRead)
                    .HasColumnType("bool")
                    .HasColumnName("is_read");
                entity.Property(e => e.CreationTime)
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("creation_time");

            });

            modelBuilder.Entity<ChatConnection>(entity =>
            {
                entity.HasKey(e => e.IdConnection)
                    .HasName("connection_pkey");

                entity.ToTable("chat_connection");

                entity.Property(e => e.IdConnection).HasColumnName("id_connection");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.UserAgent).HasColumnName("user_agent")
                    .HasColumnType("character varying");
                entity.Property(e => e.Connected)
                    .HasColumnType("bool")
                    .HasColumnName("connected");
            });

            modelBuilder.Entity<CategoryClassifier>(entity =>
            {
                entity.HasKey(e => e.IdCategoryClassifier)
                    .HasName("classifier_pkey");

                entity.ToTable("category_classifier");

                entity.Property(e => e.IdCategoryClassifier).HasColumnName("id_classifier");
                entity.Property(e => e.TypeId).HasColumnName("type_id");
                entity.Property(e => e.ClassifierName).HasColumnName("classifier_name")
                    .HasColumnType("character varying");
            });

            modelBuilder.Entity<ObjectSchedule>(entity =>
            {
                //entity.HasKey(e => e.IdCategoryClassifier)
                //    .HasName("classifier_pkey");

                entity.HasKey(e => new { e.ObjectId, e.WeekDayNum });

                entity.ToTable("object_schedule");

                entity.Property(e => e.ObjectId).HasColumnName("object_id");
                entity.Property(e => e.WeekDayNum).HasColumnName("week_day_num");
                entity.Property(e => e.IsWeekend)
                    .HasColumnType("bool")
                    .HasColumnName("is_weekend");
                entity.Property(e => e.StartTime)
                    .HasColumnType("time without time zone")
                    .HasColumnName("start_time");
                entity.Property(e => e.EndTime)
                    .HasColumnType("time without time zone")
                    .HasColumnName("end_time");
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.IdReview)
                    .HasName("review_pkey");

                entity.ToTable("review");

                entity.Property(e => e.IdReview).HasColumnName("id_review");
                entity.Property(e => e.TypeId).HasColumnName("type_id");
                entity.Property(e => e.EntityId).HasColumnName("entity_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.RatingValue).HasColumnName("rating_value");
                entity.Property(e => e.ReviewText)
                    .HasColumnType("character varying")
                    .HasColumnName("review_text");
                entity.Property(e => e.PostedDate)
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("posted_date");
                entity.Property(e => e.FScore)
                    .HasColumnType("real")
                    .HasColumnName("f_score");
            });

            modelBuilder.Entity<LikeDislike>(entity =>
            {
                entity.HasKey(e => e.IdLike)
                    .HasName("like_dislike_pkey");

                entity.ToTable("like_dislike");

                entity.Property(e => e.IdLike).HasColumnName("id_like");
                entity.Property(e => e.TypeId).HasColumnName("type_id");
                entity.Property(e => e.EntityId).HasColumnName("entity_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.IsLike)
                    .HasColumnType("bool")
                    .HasColumnName("is_like");
                entity.Property(e => e.PostedDate)
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("posted_date");
            });

            modelBuilder.Entity<ViewStatistics>(entity =>
            {
                entity.HasKey(e => e.IdView)
                    .HasName("view_statistics_pkey");

                entity.ToTable("view_statistics");

                entity.Property(e => e.IdView).HasColumnName("id_view");
                entity.Property(e => e.TypeId).HasColumnName("type_id");
                entity.Property(e => e.EntityId).HasColumnName("entity_id");
                entity.Property(e => e.VisitedTime)
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("visited_time");
                entity.Property(e => e.Ip)
                    .HasColumnType("inet")
                    .HasColumnName("ip");
                entity.Property(e => e.UserAgent)
                    .HasColumnType("character varying")
                    .HasColumnName("user_agent");
                entity.Property(e => e.UserId).HasColumnName("user_id");
            });

            modelBuilder.Entity<Favourite>(entity =>
            {
                entity.HasKey(e => e.IdFavourite)
                    .HasName("favourite_pkey");

                entity.ToTable("favourite");

                entity.Property(e => e.IdFavourite).HasColumnName("id_favourite");
                entity.Property(e => e.TypeId).HasColumnName("type_id");
                entity.Property(e => e.EntityId).HasColumnName("entity_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.AddedTime)
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("added_time");
                entity.Property(e => e.Notifications)
                    .HasColumnType("bool")
                    .HasColumnName("notifications");
            });

            modelBuilder.Entity<Sharing>(entity =>
            {
                entity.HasKey(e => e.IdSharing)
                    .HasName("sharing_pkey");

                entity.ToTable("sharing");

                entity.Property(e => e.IdSharing).HasColumnName("id_sharing");
                entity.Property(e => e.TypeId).HasColumnName("type_id");
                entity.Property(e => e.EntityId).HasColumnName("entity_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.ShareTime)
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("share_time");
                entity.Property(e => e.ShareText)
                    .HasColumnType("character varying")
                    .HasColumnName("share_text");
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.TokenStr)
                    .HasName("refresh_token_pkey");

                entity.ToTable("refresh_token");

                entity.Property(e => e.TokenStr)
                    .HasColumnType("character varying")
                    .HasColumnName("token_str");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Created)
                    .HasColumnType("timestamp")
                    .HasColumnName("created");
                entity.Property(e => e.Expires)
                    .HasColumnType("timestamp")
                    .HasColumnName("expires");
                entity.Property(e => e.Revoked)
                    .HasColumnType("timestamp")
                    .HasColumnName("revoked");
                entity.Property(e => e.ReplacedBy)
                    .HasColumnType("character varying")
                    .HasColumnName("replaced_by");
            });

            modelBuilder.Entity<Ucomment>(entity =>
            {
                entity.HasKey(e => e.IdComment)
                    .HasName("ucomment_pkey");

                entity.ToTable("ucomment");

                entity.Property(e => e.IdComment)
                    .HasColumnName("id_comment");
                entity.Property(e => e.TypeId).HasColumnName("type_id");
                entity.Property(e => e.EntityId).HasColumnName("entity_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.PostedDate)
                    .HasColumnType("timestamp with zone")
                    .HasColumnName("posted_date");
                entity.Property(e => e.ParentId).HasColumnName("parent_id");
                entity.Property(e => e.CommentText)
                    .HasColumnType("character varying")
                    .HasColumnName("comment_text");
            });

            modelBuilder.Entity<Friendship>(entity =>
            {
                entity.HasKey(e => e.IdFriendship)
                    .HasName("ucomment_pkey");

                entity.ToTable("friendship");

                entity.Property(e => e.IdFriendship)
                    .HasColumnName("id_friendship");
                entity.Property(e => e.UserFirst).HasColumnName("user_first");
                entity.Property(e => e.UserSecond).HasColumnName("user_second");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.CreatedTime)
                    .HasColumnType("timestamp with zone")
                    .HasColumnName("created_time");
                entity.Property(e => e.UpdatedTime)
                    .HasColumnType("timestamp with zone")
                    .HasColumnName("updated_time");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
