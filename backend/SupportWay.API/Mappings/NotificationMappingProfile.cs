using AutoMapper;
using SupportWay.API.DTOs;
using SupportWay.Data.Models;

namespace SupportWay.API.Mappings
{
    public class NotificationMappingProfile : AutoMapper.Profile
    {
        public NotificationMappingProfile()
        {
            CreateMap<Notification, NotificationDto>();
        }
    }
}