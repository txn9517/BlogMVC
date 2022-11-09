﻿using BlogMVC.Models;

namespace BlogMVC.Services.Interfaces
{
    // actions for working with blog posts
    public interface IBlogPostService
    {
        // Slug functionality
        public Task<bool> ValidateSlugAsync(string title, int blogPostId);

        // get all blog posts
        public Task<List<BlogPost>> GetAllBlogPostsAsync();
    }
}