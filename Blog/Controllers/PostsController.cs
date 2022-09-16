using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Blog.Models;

namespace Blog.Controllers
{
    public class PostsController : Controller
    {
        private SampleDBContext model = new SampleDBContext();

        public bool IsAdmin { get { return Session["IsAdmin"] != null && (bool)Session["IsAdmin"]; } }

        private const int PostsPerPage = 4;
        private const int postsPerFeed = 25;

        public ActionResult Index(int? id)
        {
            int pageNumber = id ?? 0;
            IEnumerable<Post> posts =
                (from post in model.Posts
                 where post.DateTime < DateTime.Now
                 orderby post.DateTime descending
                 select post).Skip(pageNumber * PostsPerPage).Take(PostsPerPage + 1);
            ViewBag.IsPreviousLinkVisible = pageNumber > 0;
            ViewBag.IsNextLinkVisible = posts.Count() > PostsPerPage;
            ViewBag.PageNumber = pageNumber;
            ViewBag.IsAdmin = IsAdmin;

            return View(posts.Take(PostsPerPage));
        }

        public ActionResult Details(int id)
        {
            Post post = GetPost(id);
            ViewBag.IsAdmin = IsAdmin;

            return View(post);
        }

        [ValidateInput(false)]
        public ActionResult Comment(int id, string name, string email, string body)
        {
            Post post = GetPost(id);
            Comment comment = new Comment();
            comment.Post = post;
            comment.DateTime = DateTime.Now;
            comment.Name = name;
            comment.Email = email;
            comment.Body = body;
            model.Comments.Add(comment);
            model.SaveChanges();

            return RedirectToAction("Details", new { id = id });
        }

        public ActionResult Tags(string id)
        {
            Tag tag = GetTag(id);
            ViewBag.IsAdmin = IsAdmin;

            return View("Index", tag.Posts);
        }

        public ActionResult Delete(int id)
        {
            if (IsAdmin)
            {
                Post post = GetPost(id);
                model.Posts.Remove(post);
                model.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult DeleteComment(int id)
        {
            if (IsAdmin)
            {
                Comment comment = model.Comments.Where(x => x.Id == id).First();

                model.Comments.Remove(comment);
                model.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            Post post = GetPost(id);
            StringBuilder tagList = new StringBuilder();
            foreach (Tag tag in post.Tags)
            {
                tagList.AppendFormat("{0}", tag.Name);
            }
            ViewBag.Tags = tagList.ToString();
            return View(post);
        }

        [ValidateInput(false)]
        public ActionResult Update(int? id, string title, string body, DateTime datetime, string tags)
        {
            if (!IsAdmin)
            {
                return RedirectToAction("Index");
            }
            Post post = GetPost(id);
            post.Title = title;
            post.Body = body;
            post.DateTime = datetime;
            post.Tags.Clear();

            tags = tags ?? string.Empty;
            string[] tagNames = tags.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string tagName in tagNames)
            {
                post.Tags.Add(GetTag(tagName));
            }
            if (!id.HasValue)
            {
                model.Posts.Add(post);
            }
            model.SaveChanges();

            return RedirectToAction("Details", new { id = post.Id });
        }

        public ActionResult RSS()
        {
            IEnumerable<SyndicationItem> posts =
                (from post in model.Posts
                 where post.DateTime < DateTime.Now
                 orderby post.DateTime descending
                 select post).Take(postsPerFeed).ToList().Select(x => GetSyndicationItem(x));

            SyndicationFeed feed = new SyndicationFeed("Onur ÜLKER", "Onur ÜLKER Blog", new Uri("http://www.yazilimciningunlugu.com"), posts);
            Rss20FeedFormatter formattedFeed = new Rss20FeedFormatter(feed);

            return new FeedResult(formattedFeed);
        }

        private SyndicationItem GetSyndicationItem(Post post)
        {
            return new SyndicationItem(post.Title, post.Body, new Uri("http://www.yazilimciningunlugu.com/posts/detail" + post.Id));
        }



        private Post GetPost(int? id)
        {
            return id.HasValue ? model.Posts.Where(x => x.Id == id).First() : new Post() { Id = -1 };
        }

        private Tag GetTag(string tagName)
        {
            return model.Tags.Where(x => x.Name == tagName).FirstOrDefault() ?? new Tag() { Name = tagName };
        }
    }
}