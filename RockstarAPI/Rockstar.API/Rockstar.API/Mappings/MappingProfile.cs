using AutoMapper;
using Rockstar.API.DTOs;
using Rockstar.API.DTOs.Auth;
using Rockstar.API.DTOs.Enrollment;
using Rockstar.API.DTOs.Schedule;
using Rockstar.API.DTOs.Subscription;
using Rockstar.API.DTOs.Trainer;
using Rockstar.API.Models;

namespace Rockstar.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>(); // ✅ Убрали PhotoBase64

            CreateMap<User, UserProfileDto>(); // ✅ Убрали PhotoBase64

            // Trainer mappings (если фото тоже убираете у тренеров)
            CreateMap<Trainer, TrainerDto>()
                .ForMember(dest => dest.DirectionName,
                    opt => opt.MapFrom(src => src.Direction != null ? src.Direction.Name : ""))
                .ForMember(dest => dest.DirectionKey,
                    opt => opt.MapFrom(src => src.Direction != null ? src.Direction.NameKey : ""));
            // ✅ Убрали PhotoBase64

            CreateMap<CreateTrainerDto, Trainer>();
            // ✅ Убрали маппинг Photo из PhotoBase64

            // Schedule mappings
            CreateMap<Schedule, ScheduleDto>()
                .ForMember(dest => dest.TrainerName,
                    opt => opt.MapFrom(src => src.Trainer != null
                        ? $"{src.Trainer.FirstName} {src.Trainer.LastName}" : ""))
                .ForMember(dest => dest.DirectionName,
                    opt => opt.MapFrom(src => src.Direction.Name))
                .ForMember(dest => dest.DirectionKey,
                    opt => opt.MapFrom(src => src.Direction.NameKey))
                .ForMember(dest => dest.ServiceName,
                    opt => opt.MapFrom(src => src.Service != null ? src.Service.Name : ""));

            CreateMap<CreateScheduleDto, Schedule>();

            // Enrollment mappings
            CreateMap<Enrollment, EnrollmentDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
                .ForMember(dest => dest.UserEmail,
                    opt => opt.MapFrom(src => src.User.Email));

            CreateMap<CreateEnrollmentDto, Enrollment>();

            // Subscription mappings
            CreateMap<Subscription, SubscriptionDto>()
                .ForMember(dest => dest.DirectionName,
                    opt => opt.MapFrom(src => src.Direction != null ? src.Direction.Name : ""))
                .ForMember(dest => dest.DirectionKey,
                    opt => opt.MapFrom(src => src.Direction != null ? src.Direction.NameKey : ""));

            CreateMap<SubscriptionPurchase, UserSubscriptionDto>()
                .ForMember(dest => dest.PurchaseId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.Subscription.Id))
                .ForMember(dest => dest.SubscriptionName, opt => opt.MapFrom(src => src.Subscription.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Subscription.Price))
                .ForMember(dest => dest.TotalSessions, opt => opt.MapFrom(src => src.Subscription.SessionsCount))
                .ForMember(dest => dest.DirectionId, opt => opt.MapFrom(src => src.Subscription.DirectionId))
                .ForMember(dest => dest.DirectionName,
                    opt => opt.MapFrom(src => src.Subscription.Direction != null
                        ? src.Subscription.Direction.Name : ""));
        }
    }
}