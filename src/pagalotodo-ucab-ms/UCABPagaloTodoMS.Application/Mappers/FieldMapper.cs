﻿using MassTransit;
using UCABPagaloTodoMS.Application.Requests;
using UCABPagaloTodoMS.Application.Responses;
using UCABPagaloTodoMS.Core.Database;
using UCABPagaloTodoMS.Core.Entities;

namespace UCABPagaloTodoMS.Application.Mappers;

public class FieldMapper
{
    public static FieldResponse MapEntityToResponse(FieldEntity entity)
    {
        var response = new FieldResponse()
        {
            Id = entity.Id,
            Name = entity.Name,
            Format = entity.Format,
            Length = entity.Length,
            AttrReference = entity.AttrReference
        };
        return response;
    }

    public static FieldEntity MapRequestToEntity(FieldRequest request, ServiceEntity serviceE)
    {
        var entity = new FieldEntity()
        {
            Name = request.Name,
            Format = request.Format,
            Length = request.Length,
            Service = serviceE, 
            AttrReference = request.AttrReference
        };
        return entity;
    }
}