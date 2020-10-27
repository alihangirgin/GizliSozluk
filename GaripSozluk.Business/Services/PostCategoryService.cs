﻿using GaripSozluk.Business.Interfaces;
using GaripSozluk.Common.ViewModels;
using GaripSozluk.Data.Domain;
using GaripSozluk.Data.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GaripSozluk.Business.Services
{
    public class PostCategoryService : IPostCategoryService
    {
        private readonly IPostCategoryRepository _postCategoryRepository;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContext;
        public PostCategoryService(IPostCategoryRepository postCategoryRepository, UserManager<User> userManager, IHttpContextAccessor httpContext)
        {
            _postCategoryRepository = postCategoryRepository;
            _userManager = userManager;
            _httpContext = httpContext;
        }

        public int GetOrCreate(string title)
        {
            var get = _postCategoryRepository.Get(x => x.Title == title);
            if(get!=null)
            {
                return get.Id;
            }
            else
            {
                string normalizedString =title;
                char[] oldValue = new char[] { 'ö', 'Ö', 'ü', 'Ü', 'ç', 'Ç', 'İ', 'ı', 'Ğ', 'ğ', 'Ş', 'ş', ' ' };
                char[] newValue = new char[] { 'o', 'O', 'u', 'U', 'c', 'C', 'I', 'i', 'G', 'g', 'S', 's', '-' };
                for (int i = 0; i < oldValue.Length; i++)
                {
                    normalizedString = normalizedString.Replace(oldValue[i], newValue[i]);
                }

                var create = new PostCategory();
                create.Title = title;
                create.NormalizedTitle = normalizedString;
                create.CreateDate = DateTime.Now;
                _postCategoryRepository.Add(create);
                _postCategoryRepository.SaveChanges();

                return create.Id;
            }
        }

        public PostCategoryViewModel AddPostCategory(PostCategoryViewModel model)
        {

            string normalizedString = model.Title;
            char[] oldValue = new char[] { 'ö', 'Ö', 'ü', 'Ü', 'ç', 'Ç', 'İ', 'ı', 'Ğ', 'ğ', 'Ş', 'ş', ' ' };
            char[] newValue = new char[] { 'o', 'O', 'u', 'U', 'c', 'C', 'I', 'i', 'G', 'g', 'S', 's', '-' };
            for (int i = 0; i < oldValue.Length; i++)
            {
                normalizedString = normalizedString.Replace(oldValue[i], newValue[i]);
            }


            var postCategory = new PostCategory()
            {
                Title = model.Title,
                NormalizedTitle = normalizedString
            };
            postCategory.CreateDate = DateTime.Now;
            //postCategory.UserId = 1;
            //postCategory.CategoryId = 1;
            //postCategory.ClickCount = 1;


            var entity = _postCategoryRepository.Add(postCategory);

            try
            {
                _postCategoryRepository.SaveChanges();
                return new PostCategoryViewModel() { Id = entity.Id, Title = entity.Title };
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                throw;
            }
        }

        //public PostViewModel UpdatePost(PostViewModel postViewModel)
        //{
        //    var model = _postRepository.Get(x => x.Id == postViewModel.Id);
        //    model.Title = postViewModel.Title;
        //    model.UpdateDate = DateTime.Now;


        //    try
        //    {
        //        var entity = _postRepository.Update(model);

        //        _postRepository.SaveChanges();
        //        return new PostViewModel() { Id = entity.Id, Title = entity.Title };
        //    }
        //    catch (Exception ex)
        //    {
        //        var errorMessage = ex.Message;
        //        throw;
        //    }
        //}

        public PostCategory Get(Expression<Func<PostCategory, bool>> expression)
        {

            return _postCategoryRepository.Get(expression);
        }

        public IQueryable<PostCategory> GetAll()
        {
            var isAdmin = false;
            var httpUser = _httpContext.HttpContext.User;
            if (httpUser.Claims.Any())
            {
                var user = _userManager.GetUserAsync(httpUser).Result;
                var roles = _userManager.GetRolesAsync(user).Result;
                if(roles.Contains("Admin"))
                {
                    isAdmin = true;
                }
            }

            if(isAdmin)
            {
                return _postCategoryRepository.GetAll();
            }
            else
            {
                return _postCategoryRepository.GetAll().Where(x=> x.Id !=8);
            }

            
        }

        public PostCategoryViewModel UpdatePostCategory(PostCategoryViewModel model)
        {
            throw new NotImplementedException();
        }


        public List<SelectListItem> selectListItem(string normalized)
        {
            var newListRow = new List<SelectListItem>();
            var categories = _postCategoryRepository.GetAll();
            foreach (var item in categories)
            {
                var newList = new SelectListItem();
                if(item.NormalizedTitle==normalized)
                {
                    newList.Selected = true;
                }
                newList.Text = item.Title;
                newList.Value = item.Id.ToString();
                newListRow.Add(newList);
            }
            return newListRow;
        }
    }
}
