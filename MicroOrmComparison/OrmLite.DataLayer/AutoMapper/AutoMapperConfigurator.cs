using System.Security.Policy;
using MicroOrmComparison.UI.Models;
using OrmLite.DataLayer.DTOs;

namespace OrmLite.DataLayer.Tests
{
    public static class AutoMapperConfigurator
    {
        public static void CreateMaps()
        {
            AutoMapper.Mapper.CreateMap<EmployeeDto, Employee>()
                .ForMember(destination => destination.Id, original => original.MapFrom(z => z.Id))
                .ForMember(destination => destination.FirstName, original => original.MapFrom(z => z.FirstName))
                .ForMember(destination => destination.LastName, originial => originial.MapFrom(z => z.LastName))
                .ForMember(destination => destination.DepartmentId, original => original.MapFrom(z => z.DepartmentId))
                .ForMember(destination => destination.Email, original => original.MapFrom(z => z.Email));


            AutoMapper.Mapper.CreateMap<Employee, EmployeeDto>()
                .ForMember(destination => destination.FirstName, original => original.MapFrom(z => z.FirstName))
                .ForMember(destination => destination.LastName, originial => originial.MapFrom(z => z.LastName))
                .ForMember(destination => destination.DepartmentId, original => original.MapFrom(z => z.DepartmentId))
                .ForMember(destination => destination.Email, original => original.MapFrom(z => z.Email));

            AutoMapper.Mapper.CreateMap<Address, AddressDto>()
                .ForMember(destination => destination.Id, original => original.MapFrom(z => z.Id))
                .ForMember(destination => destination.EmployeeId, original => original.MapFrom(z => z.EmployeeId))
                .ForMember(destination => destination.City, original => original.MapFrom(z => z.City))
                .ForMember(destination => destination.StateId, original => original.MapFrom(z => z.StateId))
                .ForMember(destination => destination.StreetAddress, original => original.MapFrom(z => z.StreetAddress))
                .ForMember(destination => destination.ZipCode, original => original.MapFrom(z => z.ZipCode));

            AutoMapper.Mapper.CreateMap<AddressDto, Address>()
                .ForMember(destination => destination.EmployeeId, original => original.MapFrom(z => z.EmployeeId))
                .ForMember(destination => destination.City, original => original.MapFrom(z => z.City))
                .ForMember(destination => destination.StateId, original => original.MapFrom(z => z.StateId))
                .ForMember(destination => destination.StreetAddress, original => original.MapFrom(z => z.StreetAddress))
                .ForMember(destination => destination.ZipCode, original => original.MapFrom(z => z.ZipCode));

            AutoMapper.Mapper.CreateMap<AssignedRolesDto, Role>();

            AutoMapper.Mapper.CreateMap<RoleDto, Role>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(z => z.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(z => z.Name));

            AutoMapper.Mapper.CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(z => z.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(z => z.Name));

            AutoMapper.Mapper.CreateMap<AssignedRolesDto, RoleDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(z => z.RoleId))
                .ForMember(dest => dest.Name, opt => opt.Ignore());
        }
    }
}
