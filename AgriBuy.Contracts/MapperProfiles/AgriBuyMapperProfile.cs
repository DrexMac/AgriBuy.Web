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

            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems))
                .ReverseMap();

            CreateMap<OrderItem, OrderItemDto>().ReverseMap();
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<ProductViewModel, ProductDto>().ReverseMap();


            CreateMap<ShoppingCart, ShoppingCartDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.Product.ImagePath))
                .ForMember(dest => dest.UnitOfMeasure, opt => opt.MapFrom(src => src.Product.UnitOfMeasure))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Product.Description))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Product.UnitPrice))
                .ForMember(dest => dest.ItemPrice, opt => opt.MapFrom(src => src.Product.UnitPrice * src.Quantity));

            CreateMap<ShoppingCartDto, ShoppingCart>();

            CreateMap<Store, StoreDto>().ReverseMap();
            CreateMap<Store, StoreViewModel>().ReverseMap();

            CreateMap<User, UserDto>().ReverseMap();
        }
    }
}
