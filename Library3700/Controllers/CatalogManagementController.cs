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
using Library3700.Models.Objects.Account;

namespace Library3700.Controllers
{
    public class CatalogManagementController : Controller
    {

        NotificationController notification = new NotificationController();
        /// <summary>
        ///         Index that will return a list of library items with the information to view
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

        /// <summary>
        /// post method to create an item 
        /// </summary>
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
                    //determine if item is a book or audiobook
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
                    //if object modelstate is valid then add item and save to db
                    if (ModelState.IsValid)
                    {
                        db.Items.Add(item);
                        db.SaveChanges();
                        //create an item status log for the added item
                        ItemStatusLog updateStatus = new ItemStatusLog
                        {
                            ItemId = item.ItemId,
                            AccountId = ((AccountAdapter)System.Web.HttpContext.Current.Session["activeAccount"]).AccountNumber,
                            ItemStatusTypeId = 1,
                            ItemStatusType = db.ItemStatusTypes.Where(x => x.ItemStatusTypeId == 1).FirstOrDefault(),
                            LogDateTime = DateTime.Now
                        };
                        db.ItemStatusLogs.Add(updateStatus);
                        db.SaveChanges();

                        return notification.SuccessItemCreation();
                    }
                }
                //catch any exceptions and return them
                catch (DataException e)
                {
                    return notification.FailureItemCreation();
                }
                return View(item);
            }
        }

        /// <summary>
        /// finds an item by the item id and returns the item to the details view
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
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

        /// <summary>
        /// returns the item to to be edited to the view
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// saves the item after it has been edited to the database
        /// </summary>
        /// <param name="catalogItem"></param>
        /// <returns></returns>
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
        /// deletes the item logs for an item and then deletes the item
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

        /// <summary>
        /// gets the item checkout view and returns a list of items and accounts the item can be checked 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CheckoutItem()
        {
            using (LibraryEntities db = new LibraryEntities())
            {
                try
                {
                    //TODO: get the last item status and not all of the item status
                    ItemStatusViewModel AccountItemsCheckout = new ItemStatusViewModel();
                    List<Item> ItemList = db.ItemStatusLogs.Select(f => f.Item).ToList();
                    List<Item> NewItemList = new List<Item>();

                    foreach (var item in ItemList)
                    {
                        Item listItem = ItemList.Where(x => x.ItemId == item.ItemId).Last();
                        byte lastStatus = listItem.ItemStatusLogs.Select(f => f.ItemStatusTypeId).Last();
                        if (lastStatus == 1)
                        {
                            NewItemList.Add(listItem);
                        }
                    }
                    List<Account> AccountList = db.Accounts.Where(x => x.IsLibrarian == false).ToList();
                    AccountItemsCheckout.ItemList = NewItemList;
                    AccountItemsCheckout.AccountList = AccountList;
                    AccountItemsCheckout.ItemID = -1;
                    AccountItemsCheckout.AccountID = -1;
                    return View(AccountItemsCheckout);
                }
                catch
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
            }
        }

        /// <summary>
        /// this method return a differnt view depending on what status update the item is needing
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UpdateItemStatus(int id)
        {
            using (LibraryEntities db = new LibraryEntities())
            {
                if (id == 1)
                {
                    ItemStatusViewModel AccountItemsCheckout = new ItemStatusViewModel();
                    List<Item> ItemList = db.ItemStatusLogs.Select(f => f.Item).ToList();
                    List<Item> NewItemList = new List<Item>();

                    foreach (var item in ItemList)
                    {
                        Item listItem = ItemList.Where(x => x.ItemId == item.ItemId).Last();
                        byte lastStatus = listItem.ItemStatusLogs.Select(f => f.ItemStatusTypeId).Last();
                        if (lastStatus != 1)
                        {
                            NewItemList.Add(listItem);
                        }
                    }
                    var activeAccount = (AccountAdapter)System.Web.HttpContext.Current.Session["activeAccount"];
                    AccountItemsCheckout.ItemList = NewItemList;
                    AccountItemsCheckout.ItemID = -1;
                    AccountItemsCheckout.AccountID = activeAccount.AccountNumber;
                    return View("CheckInItem", AccountItemsCheckout);
                }
                if (id == 2)
                {
                    ItemStatusViewModel AccountItemsCheckout = new ItemStatusViewModel();
                    List<Item> ItemList = db.ItemStatusLogs.Select(f => f.Item).ToList();
                    List<Item> NewItemList = new List<Item>();

                    foreach (var item in ItemList)
                    {
                        Item listItem = ItemList.Where(x => x.ItemId == item.ItemId).Last();
                        byte lastStatus = listItem.ItemStatusLogs.Select(f => f.ItemStatusTypeId).Last();
                        if (lastStatus == 1)
                        {
                            NewItemList.Add(listItem);
                        }
                    }
                    var activeAccount = (AccountAdapter)System.Web.HttpContext.Current.Session["activeAccount"];
                    AccountItemsCheckout.ItemList = NewItemList;
                    AccountItemsCheckout.ItemID = -1;
                    AccountItemsCheckout.AccountID = activeAccount.AccountNumber;
                    return View("ReserveItem", AccountItemsCheckout);
                }
                if (id == 3)
                {
                   
                    ItemStatusViewModel AccountItemsCheckout = new ItemStatusViewModel();
                    var activeAccount = (AccountAdapter)System.Web.HttpContext.Current.Session["activeAccount"];
                    List<Item> accountItems = db.ItemStatusLogs.Where(x => x.AccountId == activeAccount.AccountNumber).Select(f => f.Item).ToList();
                    List<Item> NewItemList = new List<Item>();

                    foreach (var item in accountItems)
                    {
                        Item listItem = accountItems.Where(x => x.ItemId == item.ItemId).Last();
                        byte lastStatus = listItem.ItemStatusLogs.Select(f => f.ItemStatusTypeId).Last();
                        if (lastStatus == 4)
                        {
                            NewItemList.Add(listItem);
                        }
                    }
                    AccountItemsCheckout.ItemList = accountItems;
                    AccountItemsCheckout.ItemID = -1;
                    AccountItemsCheckout.AccountID = activeAccount.AccountNumber;

                    return View("MissingItem", AccountItemsCheckout);
                }
                return View();
            }
        }

        /// <summary>
        /// this method takes in an int id sent from the view to update an item's status
        /// </summary>
        /// <param name="itemstatusviewmodel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateItemStatus(ItemStatusViewModel itemstatusviewmodel)
        {
            using (LibraryEntities db = new LibraryEntities())
            {
                try
                {
                    if (itemstatusviewmodel.itemStatusTypeID == 2)
                    {
                        var currentStatus = db.ItemStatusLogs
                            .Where(x => x.ItemId == itemstatusviewmodel.ItemID)
                            .OrderByDescending(x => x.LogDateTime)
                            .Select(x => x.ItemStatusTypeId)
                            .FirstOrDefault();

                        if (currentStatus == 2)
                        {
                            return null;
                        }
                    }

                    Item item = db.Items.Find(itemstatusviewmodel.ItemID);
                    Account account = db.Accounts.Find(itemstatusviewmodel.AccountID);
                    ItemStatusType itemStatusType = db.ItemStatusTypes.Find(itemstatusviewmodel.itemStatusTypeID);

                    ItemStatusLog itemStatusLog = new ItemStatusLog
                    {
                        Item = item,
                        ItemId = item.ItemId,
                        Account = account,
                        AccountId = account.AccountId,
                        ItemStatusTypeId = itemStatusType.ItemStatusTypeId,
                        LogDateTime = DateTime.Now
                    };
                    if (itemStatusLog.ItemStatusTypeId == 4)
                    {
                        DateTime Now = DateTime.Now;
                        DateTime Duedate = Now.AddDays(3);
                        itemStatusLog.ReturnItemDueDate = Duedate;
                        db.ItemStatusLogs.Add(itemStatusLog);
                        db.SaveChanges();

                        return notification.CheckoutSuccess(Duedate);
                    }
                    if(itemStatusLog.ItemStatusTypeId == 2)
                    {
                            DateTime Now = DateTime.Now;
                            DateTime HoldDate = Now.AddDays(1);
                            itemStatusLog.ReturnItemDueDate = HoldDate;
                            db.ItemStatusLogs.Add(itemStatusLog);
                            db.SaveChanges();

                            return notification.ReserveItemSuccess(HoldDate);
                   }
                    else
                    {
                        db.ItemStatusLogs.Add(itemStatusLog);
                        db.SaveChanges();

                        return notification.UpdateItemSuccess();
                    }
                }
                catch
                {
                    return notification.UpdateItemFailure();
                }
            }
        }

        /// <summary>
        /// this private method will generate a list of items that a patron has checked out or reserved
        /// </summary>
        /// <param name="accountID"></param>
        /// <returns></returns>
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

        /// <summary>
        /// this method will return to the view the patron's items
        /// </summary>
        /// <returns></returns>
        public ActionResult PatronItems()
        {
            var activeAccount = (AccountAdapter)System.Web.HttpContext.Current.Session["activeAccount"];
            List<AccountItems> patronItems = GeneratePatronItemsList(activeAccount.AccountNumber);
            return View(patronItems);
        }

        /// <summary>
        /// this method is returns a missing item list for the librarian to view
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles ="librarian")]
        public ActionResult MissingItemList()
        {
            using (LibraryEntities db = new LibraryEntities())
            {
                try
                {
                    ItemStatusViewModel AccountItemsCheckout = new ItemStatusViewModel();

                    List<Item> ItemList = db.Items.ToList();
                    List<Item> NewItemList = new List<Item>();

                    foreach (var item in ItemList)
                    {
                        Item listItem = ItemList.Where(x => x.ItemId == item.ItemId).Last();
                        byte lastStatus = listItem.ItemStatusLogs.Select(f => f.ItemStatusTypeId).Last();
                        if (lastStatus == 3)
                        {
                            NewItemList.Add(listItem);
                        }
                    }
                    AccountItemsCheckout.MissingItemList = NewItemList;
                    AccountItemsCheckout.ItemStatusText = db.ItemStatusTypes.Where(x => x.ItemStatusTypeId == 3).Select(x => x.ItemStatusName).FirstOrDefault();

                    return View("MissingItemList", AccountItemsCheckout);
                }
                catch
                {
                    return null;
                }
            }
        }


    }
}
           