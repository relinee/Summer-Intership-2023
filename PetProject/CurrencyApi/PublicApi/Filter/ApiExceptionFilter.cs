﻿using Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Filter;

public class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }
    
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case ApiRequestLimitException:
            {
                _logger.LogError(context.Exception, context.Exception.Message);
                var result = new ObjectResult("Все доступные запросы исчерпаны")
                {
                    StatusCode = StatusCodes.Status429TooManyRequests
                };
                context.ExceptionHandled = true;
                context.Result = result;
                break;
            }
            case CurrencyNotFoundException:
            {
                var result = new ObjectResult("Попытка выполнения запроса с неизвестной валютой")
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
                context.ExceptionHandled = true;
                context.Result = result;
                break;
            }
            case CurrencyFavouriteNotFoundException:
            {
                var result = new ObjectResult("Такого избранного не существует")
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
                context.ExceptionHandled = true;
                context.Result = result;
                break;
            }
            case CurrencyFavouriteIsAlreadyExistError:
            {
                var result = new ObjectResult("Избранное с такими параметрами уже существует")
                {
                    StatusCode = StatusCodes.Status409Conflict
                };
                context.ExceptionHandled = true;
                context.Result = result;
                break;
            }
            case RpcException rpcException:
            {
                _logger.LogError(context.Exception, context.Exception.Message);
                var result = new ObjectResult("Ошибка внутреннего сервиса")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                if (rpcException.StatusCode == StatusCode.ResourceExhausted)
                {
                    result = new ObjectResult("Все доступные запросы исчерпаны")
                    {
                        StatusCode = StatusCodes.Status429TooManyRequests
                    };
                }
                context.ExceptionHandled = true;
                context.Result = result;
                break;
            }
            default:
            {
                _logger.LogError(context.Exception, context.Exception.Message);
                var result = new ObjectResult("Внутренняя ошибка сервера")
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                context.ExceptionHandled = true;
                context.Result = result;
                break;
            }
        }
    }
}