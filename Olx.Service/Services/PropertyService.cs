﻿using Microsoft.EntityFrameworkCore;
using Olx.DataAccess.IRepositories;
using Olx.Domain.Entities;
using Olx.Service.DTOs.Properties;
using Olx.Service.Exceptions;
using Olx.Service.Extentions;
using Olx.Service.Interfaces;

namespace Olx.Service.Services;

public class PropertyService : IPropertyService
{
    private readonly IRepository<Property> propertyRepository;

    public PropertyService(IRepository<Property> propertyRepository)
    {
        this.propertyRepository = propertyRepository;
    }

    public async Task<PropertyViewDto> CreateAsync(PropertyCreateDto property)
    {
        var existProperty = await propertyRepository
                              .SelectAllAsQueryable()
                              .FirstOrDefaultAsync(p => p.Name == property.Name && p.CategoryId == property.CategoryId);
        if (existProperty != null && existProperty.IsDeleted)
            return await UpdateAsync(existProperty.Id, property.MapTo<PropertyUpdateDto>(), true);

        if (existProperty != null)
            throw new CustomException(409, "Property already exist");

        existProperty = property.MapTo<Property>();
        var createProperty = await propertyRepository.InsertAsync(existProperty);
        await propertyRepository.SaveAsync();

        return createProperty.MapTo<PropertyViewDto>();
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var existProperty = await propertyRepository.SelectByIdAsync(id)
            ?? throw new CustomException(404, "Property not found");

        existProperty.IsDeleted = true;
        existProperty.DeletedAt = DateTime.UtcNow;

        await propertyRepository.DeleteAsync(existProperty);
        await propertyRepository.SaveAsync();

        return true;
    }

    public async Task<IEnumerable<PropertyViewDto>> GetAllAsync()
    {
        return await Task.FromResult(propertyRepository.SelectAllAsQueryable()
            .Where(c => !c.IsDeleted)
            .MapTo<PropertyViewDto>());
    }

    public async Task<PropertyViewDto> GetByIdAsync(long id)
    {
        var existProperty = await propertyRepository.SelectByIdAsync(id)
            ?? throw new CustomException(404, "Property not found");

        return existProperty.MapTo<PropertyViewDto>();
    }

    public async Task<PropertyViewDto> UpdateAsync(long id, PropertyUpdateDto property, bool isDeleted = false)
    {
        var existProperty = new Property();

        if (isDeleted)
        {
            existProperty = await propertyRepository
                              .SelectAllAsQueryable()
                              .FirstOrDefaultAsync(p => p.Id == id);
            existProperty.IsDeleted = false;
        }

        existProperty.Name = property.Name;
        existProperty.CategoryId = property.CategoryId;
        existProperty.UpdatedAt = DateTime.UtcNow;

        await propertyRepository.UpdateAsync(existProperty);
        await propertyRepository.SaveAsync();

        return existProperty.MapTo<PropertyViewDto>();
    }
}