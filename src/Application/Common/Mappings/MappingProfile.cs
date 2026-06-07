using AutoMapper;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Transaction, TransactionDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.CategoryColor, o => o.MapFrom(s => s.Category.Color))
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.TransactionDate, o => o.MapFrom(s => s.TransactionDate.ToString("yyyy-MM-dd")));

        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

        CreateMap<MonthlyBudget, BudgetDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.CategoryColor, o => o.MapFrom(s => s.Category.Color))
            .ForMember(d => d.BudgetedAmount, o => o.MapFrom(s => s.Amount));
    }
}
