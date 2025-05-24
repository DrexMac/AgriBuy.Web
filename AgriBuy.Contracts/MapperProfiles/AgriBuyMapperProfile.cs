using AgriBuy.Contracts.Dto;
using AgriBuy.Models.Models;
using AgriBuy.Models.ViewModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriBuy.Contracts.MapperProfiles
{
    public class AgriBuyMapperProfile : Profile
    {
        public AgriBuyMapperProfile()
        {
            CreateMap<LoginInfo, LoginInfoDto>().ReverseMap();
            CreateMap<Order, OrderDto>().ReverseMap();
            CreateMap<OrderItem, OrderItemDto>().ReverseMap();
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<ProductViewModel, ProductDto>().ReverseMap();
            CreateMap<ShoppingCart, ShoppingCartDto>().ReverseMap();
            CreateMap<Store, StoreDto>().ReverseMap();
            CreateMap<User, UserDto>().ReverseMap();

            CreateMap<User, UserDto>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
               .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
               .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.EmailAddress))
               .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
               .ReverseMap();
        }
    }
}
