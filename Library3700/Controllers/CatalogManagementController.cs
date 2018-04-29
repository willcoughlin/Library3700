﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Library3700.Models;
using Library3700.Models.ViewModels;
using Library3700.Controllers;


namespace Library3700.Controllers
{
    public class CatalogManagementController : Controller
    {

        NotificationController notification = new NotificationController();
        /// <summary>
        ///         Index that will return a list of library books with the information to view
        /// </summary>
        public ActionResult Index()
        {
            using (

                LibraryEntities db = new LibraryEntities())
            {
                try
                {
                    List<Item> items = db.Items.Where(x => x.ItemId != 0).ToList();
                    CatalogItemViewModel catalogItemViewModel = new CatalogItemViewModel();
                    catalogItemViewModel.catalogItemList = items;

                    return View(catalogItemViewModel);
                }
                catch
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);

                }

            }
        }

        /// <summary>
        ///  Get create item view
        /// </summary>
        [HttpGet]
        public ActionResult CreateItem()
        {
            return View();
        }

        //post method to create an item
        [HttpPost]
        public ActionResult CreateItem(CatalogItemViewModel catalogItem)
        {
            //create new item
            Item item = new Item();
            using (LibraryEntities db = new LibraryEntities())
            {
                try
                {
                    //set item equal to view model object
                    item.Author = catalogItem.Author;
                    item.Title = catalogItem.Title;
                    item.Genre = catalogItem.Genre;
                    item.PublicationYear = catalogItem.PublicationYear;
                    if (catalogItem.ItemTypeName.ToLower() == "book")
                    {
                        catalogItem.ItemTypeId = 1;
                        item.ItemTypeId = catalogItem.ItemTypeId;
                        item.ItemType = db.ItemTypes.Where(x => x.ItemTypeId == catalogItem.ItemTypeId).FirstOrDefault();
                    }
                    if (catalogItem.ItemTypeName.ToLower() == "audiobook")
                    {
                        catalogItem.ItemTypeId = 2;
                        item.ItemTypeId = catalogItem.ItemTypeId;
                        item.ItemType = db.ItemTypes.Where(x => x.ItemTypeId == catalogItem.ItemTypeId).FirstOrDefault();
                    }
                    if (ModelState.IsValid)
                    {
                        db.Items.Add(item);
                        db.SaveChanges();
                        return notification.SuccessItemCreation();
                    }
                }
                catch (DataException)
                {
                    return notification.FailureItemCreation();
                }
                return View(item);
            }
        }


        [HttpGet]
        //finds an item by the item id and returns the item to the details view
        public ActionResult ItemDetails(int id)
        {
            using (LibraryEntities db = new LibraryEntities())
            {
                try
                {
                    if (id == 0)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                    }
                    CatalogItemViewModel catalogItemViewModel = new CatalogItemViewModel();
                    catalogItemViewModel.Item = db.Items.Find(id);
                    catalogItemViewModel.ItemTypeName = db.Items.Where(x => x.ItemTypeId == x.ItemType.ItemTypeId).Select(f => f.ItemType.ItemTypeName).FirstOrDefault();
                    if (catalogItemViewModel == null)
                    {
                        return HttpNotFound();
                    }
                    return View(catalogItemViewModel);
                }
                catch
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
            }
        }

        [HttpGet]
        public ActionResult EditItem(int id)
        {
            using (LibraryEntities db = new LibraryEntities())
            {
                try
                {
                    if (id == 0)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                    }
                    CatalogItemViewModel catalogItemViewModel = new CatalogItemViewModel();
                    catalogItemViewModel.Item = db.Items.Find(id);
                    catalogItemViewModel.ItemTypeName = db.Items.Where(x => x.ItemTypeId == x.ItemType.ItemTypeId).Select(f => f.ItemType.ItemTypeName).FirstOrDefault();
                    if (catalogItemViewModel == null)
                    {
                        return HttpNotFound();
                    }
                    return View(catalogItemViewModel);
                }
                catch
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
            }

        }


        [HttpPost]
        public ActionResult EditItem(CatalogItemViewModel catalogItem)
        {
            using (LibraryEntities db = new LibraryEntities())
            {
                //create new item to save edit item too
                Item item = db.Items.Find(catalogItem.ItemID);

                try
                {
                    //set item equal to view model object
                    item.Author = catalogItem.Author;
                    item.Title = catalogItem.Title;
                    item.Genre = catalogItem.Genre;
                    item.PublicationYear = catalogItem.PublicationYear;
                    if (catalogItem.ItemTypeName.ToLower() == "book")
                    {
                        catalogItem.ItemTypeId = 1;
                        item.ItemTypeId = catalogItem.ItemTypeId;
                        item.ItemType = db.ItemTypes.Where(x => x.ItemTypeId == catalogItem.ItemTypeId).FirstOrDefault();
                    }
                    if (catalogItem.ItemTypeName.ToLower() == "audiobook")
                    {
                        catalogItem.ItemTypeId = 2;
                        item.ItemTypeId = catalogItem.ItemTypeId;
                        item.ItemType = db.ItemTypes.Where(x => x.ItemTypeId == catalogItem.ItemTypeId).FirstOrDefault();
                    }
                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();
                        return notification.EditItemSuccess();
                    }
                }
                catch
                {
                    return notification.EditItemFailure();
                }
                return View(item);
            }
        }



        /// <summary>
        /// arhcives an item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeleteItem(int id)
        {
            using (LibraryEntities db = new LibraryEntities())
            {
                try
                {
                    Item item = db.Items.Find(id);
                    List<ItemStatusLog> itemStatusList = db.ItemStatusLogs.Where(x => x.ItemId == id).ToList();
                    foreach (var itemstatus in itemStatusList)
                    {
                        db.ItemStatusLogs.Remove(itemstatus);
                        db.SaveChanges();
                    }
                    db.Items.Remove(item);
                    db.SaveChanges();
                    return notification.DeleteItemSuccess();
                }
                catch
                {
                    return notification.DeleteItemFailure();
                }
            }
        }

        [HttpGet]
        public ActionResult UpdateItemStatus()
        {
            using (LibraryEntities db = new LibraryEntities())
            {
                ItemStatusViewModel itemstatusviewmodel = new ItemStatusViewModel();
                if (itemstatusviewmodel.ItemStatusType.ItemStatusTypeId == 1)
                {
                    return View();
                }
                if (itemstatusviewmodel.ItemStatusType.ItemStatusTypeId == 2)
                {
                    return View();
                }
                if (itemstatusviewmodel.ItemStatusType.ItemStatusTypeId == 3)
                {
                    return View();
                }
                if (itemstatusviewmodel.ItemStatusType.ItemStatusTypeId == 4)
                {
                    return View();
                }
                return View();
            }
        }
        [HttpPost]
        public ActionResult UpdateItemStatus(ItemStatusViewModel itemstatusviewmodel)
        {
            using (LibraryEntities db = new LibraryEntities())
            {
                try
                {
                    
                    Item item = db.Items.Find(itemstatusviewmodel.ItemID);
                    Account account = db.Accounts.Find(itemstatusviewmodel.AccountID);
                    ItemStatusType itemStatusType = db.ItemStatusTypes.Find(itemstatusviewmodel.itemStatusTypeID);
                    
                    ItemStatusLog itemStatusLog = new ItemStatusLog {
                    Item = item,
                    ItemId = item.ItemId,
                    Account = account,
                    AccountId = account.AccountId,
                    ItemStatusTypeId = itemStatusType.ItemStatusTypeId,
                    LogDateTime = DateTime.Now
                    };
                    db.ItemStatusLogs.Add(itemStatusLog);
                    db.SaveChanges();

                    return Json(new { success = true });
                }
                catch
                {
                    return Json(new { success = false });
                }
            }
        }

        private static List<AccountItems> GeneratePatronItemsList(int accountID)
        {
            using (LibraryEntities db = new LibraryEntities())
            {
                try
                {
                    List<ItemStatusLog> accountitemList = db.ItemStatusLogs.Where(x => x.AccountId == accountID && (x.ItemStatusTypeId == 2 || x.ItemStatusTypeId == 4)).ToList();
                    List<AccountItems> accountItems = new List<AccountItems>();

                    if(accountitemList != null || accountitemList.Count != 0)
                    {
                        foreach (var i in accountitemList)
                        {
                            AccountItems accountitem = new AccountItems
                            {
                                item = i.Item,
                                accountID = i.AccountId,
                                account = i.Account,
                                itemTypeID = i.ItemStatusTypeId,
                                itemStatusText = i.ItemStatusType.ItemStatusName
                            };
                            accountItems.Add(accountitem);
                        }
                    }

                    return accountItems;
                }
                catch
                {
                    return null;
                }
            }
        }

        public ActionResult PatronItems(int accountID)
        {
            List<AccountItems> patronItems = GeneratePatronItemsList(accountID);
            return View(patronItems);
        }
    }
}
           