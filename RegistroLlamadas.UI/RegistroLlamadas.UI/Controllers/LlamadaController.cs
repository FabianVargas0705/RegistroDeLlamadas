using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegistroLlamadas.UI.Models;

namespace RegistroLlamadas.UI.Controllers
{
    public class LlamadaController : Controller
    {
        // GET: LlamadaController
        
        public ActionResult Llamadas()
        {
            List<LlamadaModel> llamadas = new List<LlamadaModel>()
            {
                new LlamadaModel() { IdLlamada = 1},
                new LlamadaModel() { IdLlamada = 2},
                new LlamadaModel() { IdLlamada = 3}
            };

            return View(llamadas); 
        }

        // GET: LlamadaController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: LlamadaController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LlamadaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LlamadaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LlamadaController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LlamadaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LlamadaController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
