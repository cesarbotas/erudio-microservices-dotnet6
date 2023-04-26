﻿using AutoMapper;
using GeekShopping.CartAPI.Data.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;

namespace GeekShopping.CartAPI.Repository
{
    public class CouponRepository : ICouponRepository
    {
        private readonly HttpClient _client;

        public const string BasePathCoupon = "api/v1/coupon";

        public CouponRepository(HttpClient client)
        {
            _client = client;
        }

        public async Task<CouponVO> GetCoupon(string couponCode, string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync($"{BasePathCoupon}/{couponCode}");

            if (response.StatusCode != HttpStatusCode.OK) return new CouponVO();

            var content = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<CouponVO>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}