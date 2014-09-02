using System.Collections.Generic;
using AutoMapper;

namespace EntityFramework.DataLayer.Automapper
{
    internal static class AutoMapperConfigurator
    {
        public static void CreateMaps()
        {
            Mapper.CreateMap<Employee, MicroOrmComparison.UI.Models.Employee>()
                .ForMember(destination => destination.Id, original => original.MapFrom(z => z.Id))
                .ForMember(destination => destination.FirstName, original => original.MapFrom(z => z.FirstName))
                .ForMember(destination => destination.LastName, originial => originial.MapFrom(z => z.LastName))
                .ForMember(destination => destination.DepartmentId, original => original.MapFrom(z => z.DepartmentId))
                .ForMember(destination => destination.Email, original => original.MapFrom(z => z.Email))
                .ForMember(destination => destination.Addresses, original => original.MapFrom(src => Mapper.Map<ICollection<Address>, List<MicroOrmComparison.UI.Models.Address>>(src.Addresses)))
                .ForMember(destination => destination.Roles, original => original.MapFrom(src => Mapper.Map<ICollection<AssignedRole>, List<MicroOrmComparison.UI.Models.Role>>(src.AssignedRoles)));


            Mapper.CreateMap<MicroOrmComparison.UI.Models.Employee, Employee>()
                .ForMember(destination => destination.Id, original => original.MapFrom(z => z.Id))
                .ForMember(destination => destination.FirstName, original => original.MapFrom(z => z.FirstName))
                .ForMember(destination => destination.LastName, originial => originial.MapFrom(z => z.LastName))
                .ForMember(destination => destination.DepartmentId, original => original.MapFrom(z => z.DepartmentId))
                .ForMember(destination => destination.Email, original => original.MapFrom(z => z.Email))
                .ForMember(destination => destination.Addresses, original => original.MapFrom(src => Mapper.Map<List<MicroOrmComparison.UI.Models.Address>, ICollection<Address>>(src.Addresses)))
                .ForMember(destination => destination.AssignedRoles, original => original.MapFrom(src => Mapper.Map<List<MicroOrmComparison.UI.Models.Role>, ICollection<AssignedRole>>(src.Roles)));

            Mapper.CreateMap<Address, MicroOrmComparison.UI.Models.Address>()
                .ForMember(destination => destination.Id, original => original.MapFrom(z => z.Id))
                .ForMember(destination => destination.StreetAddress, original => original.MapFrom(z => z.StreetAddress))
                .ForMember(destination => destination.City, original => original.MapFrom(z => z.City))
                .ForMember(destination => destination.EmployeeId, original => original.MapFrom(z => z.EmployeeId))
                .ForMember(destination => destination.StateId, original => original.MapFrom(z => z.StateId))
                .ForMember(destination => destination.ZipCode, original => original.MapFrom(z => z.ZipCode));

            Mapper.CreateMap<MicroOrmComparison.UI.Models.Address, Address>()
                .ForMember(destination => destination.Id, original => original.MapFrom(z => z.Id))
                .ForMember(destination => destination.StreetAddress, original => original.MapFrom(z => z.StreetAddress))
                .ForMember(destination => destination.City, original => original.MapFrom(z => z.City))
                .ForMember(destination => destination.EmployeeId, original => original.MapFrom(z => z.EmployeeId))
                .ForMember(destination => destination.StateId, original => original.MapFrom(z => z.StateId))
                .ForMember(destination => destination.ZipCode, original => original.MapFrom(z => z.ZipCode));

            Mapper.CreateMap<MicroOrmComparison.UI.Models.Role, AssignedRole>()
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(z => z.Id));
                //.ForMember(dest => dest.Role.Name, opt => opt.MapFrom(z => z.Name));

            Mapper.CreateMap<AssignedRole, MicroOrmComparison.UI.Models.Role>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(z => z.RoleId));
                //.ForMember(dest => dest.Name, opt => opt.MapFrom(z => z.Role.Name));

            Mapper.CreateMap<MicroOrmComparison.UI.Models.Role, Role>()
                .ForMember(destination => destination.Id, opt => opt.MapFrom(z => z.Id))
                .ForMember(destination => destination.Name, opt => opt.MapFrom(z => z.Name));

            Mapper.CreateMap<Role, MicroOrmComparison.UI.Models.Role>()
                .ForMember(destination => destination.Id, opt => opt.MapFrom(z => z.Id))
                .ForMember(destination => destination.Name, opt => opt.MapFrom(z => z.Name));
        }
    }
}
