﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BeerShop.Models;
using PagedList;
using BeerShop.Models;

namespace BeerShop.Controllers
{
    public class ItemsController : Controller
    {
        private BeerShopContext db = new BeerShopContext();

        //
        // GET: /Items/

        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page,string beerCountry,string beerType )
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Name desc" : "";
            ViewBag.PriceSort = sortOrder == "Price" ? "Price desc" : "Price" ;
            ViewBag.beerCountry = beerCountry;
            ViewBag.beerType = beerType;

            if (Request.HttpMethod == "GET")
            {
                searchString = currentFilter;
            }
            else
            {
                page = 1;
            }
            ViewBag.CurrentFilter = searchString;
            var items = from i in db.Items select i;
            if (beerCountry != null)
            {
                items = (items.Where(i => i.categories.Contains(db.CategoryItems.FirstOrDefault(c => c.name == beerCountry))));
            }
            if (beerType != null)
            {
                items = (items.Where(i => i.categories.Contains(db.CategoryItems.FirstOrDefault(c => c.name == beerType))));
            }

           
            if (!String.IsNullOrEmpty(searchString))
            {
               items = items.Where(s => s.name.ToUpper().Contains(searchString.ToUpper()) );
            }

            switch (sortOrder)
            {
                case  "Name desc":
                    items = items.OrderByDescending(i => i.name);
                    break;
                case "Price":
                    items= items.OrderByDescending(i => i.Price);
                    break;
                case "Price desc" :
                    items= items.OrderBy(i => i.Price);
                    break;
                default :
                    items = items.OrderBy(i => i.name);
                    break;
            }


            int PageSize = 3;
            int pagenumber = (page ?? 1);

            ViewBag.PermissionLevel = Worker.masterPermission;


            var categories = from c in db.Categories select c;

            
            return View(items.ToPagedList(pagenumber,PageSize));

        }
        
        //
        // GET: /Items/Details/5

        public ActionResult Details(int id = 0)
        {
            Item item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            ItemCategoryHelper itemHelper = new ItemCategoryHelper();
            itemHelper.item = item;
            itemHelper.SelectedCountry = item.categories.FirstOrDefault(c => c.category.name.Equals("by Country")).name.ToString();
            itemHelper.SelectedType = item.categories.FirstOrDefault(c => c.category.name.Equals("by Type")).name.ToString();
            return View(itemHelper);
        }

        //
        // GET: /Items/Create

        public ActionResult Create()
        {

           // Where(c => c.category.name.Equals("by Country")).
            
            var countryList = db.CategoryItems.Where(c => c.category.name.Equals("by Country"));
            SelectList SelectCategoryList = new SelectList(countryList, "CategoryItemID", "name");

            ViewBag.countriesList = SelectCategoryList;

            var typesList = db.CategoryItems.Where(c => c.category.name.Equals("by Type"));
            SelectList SelectTypeList = new SelectList(typesList, "CategoryItemID", "name");
            ViewBag.typesList = SelectTypeList;
            return View();
        }

        //
        // POST: /Items/Create

        [HttpPost]
        public ActionResult Create(ItemCategoryHelper itemHelper)
        {
            if (ModelState.IsValid)
            {
                itemHelper.item.isStillOnSale = true;
                db.Items.Add(itemHelper.item);
                int countryID = int.Parse(itemHelper.SelectedCountry);
                int typeID = int.Parse(itemHelper.SelectedType);
                db.CategoryItems.FirstOrDefault(c => c.CategoryItemID == countryID).items.Add(itemHelper.item);
                db.CategoryItems.FirstOrDefault(c => c.CategoryItemID == typeID).items.Add(itemHelper.item);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(itemHelper.item);
        }

        //
        // GET: /Items/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Item item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            ItemCategoryHelper itemHelper = new ItemCategoryHelper();
            itemHelper.item = item;
            var selectedCountry = item.categories.FirstOrDefault(c => c.category.name.Equals("by Country"));
            var selectedType = item.categories.FirstOrDefault(c => c.category.name.Equals("by Type"));

            var countryList = db.CategoryItems.Where(c => c.category.name.Equals("by Country"));
            SelectList SelectCategoryList = new SelectList(countryList, "CategoryItemID", "name",selectedCountry.CategoryItemID);
            foreach (var i in SelectCategoryList)


            ViewBag.countriesList = SelectCategoryList;

            var typesList = db.CategoryItems.Where(c => c.category.name.Equals("by Type"));
            SelectList SelectTypeList = new SelectList(typesList, "CategoryItemID", "name",selectedType.CategoryItemID.ToString());

            ViewBag.typesList = SelectTypeList;

            return View(itemHelper);
        }

        //
        // POST: /Items/Edit/5

        [HttpPost]
        public ActionResult Edit(ItemCategoryHelper itemHelper)
        {
            if (ModelState.IsValid)
            {
                db.Entry(itemHelper.item).State = EntityState.Modified;
                int countryID = int.Parse(itemHelper.SelectedCountry);
                int typeID = int.Parse(itemHelper.SelectedType);
                db.CategoryItems.FirstOrDefault(c => c.CategoryItemID == countryID).items.Add(itemHelper.item);
                db.CategoryItems.FirstOrDefault(c => c.CategoryItemID == typeID).items.Add(itemHelper.item);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(itemHelper.item);
        }

        //
        // GET: /Items/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Item item = db.Items.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        //
        // POST: /Items/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Item item = db.Items.Find(id);
            db.Items.Remove(item);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}