using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("v1/products")]
public class ProductController : ControllerBase
{
    [HttpGet]
    [Route("")]
    [AllowAnonymous]
    public async Task<ActionResult<List<Product>>> Get([FromServices] DataContext context)
    {
        var products = await context.Products.Include(x => x.Category).AsNoTracking().ToListAsync();
        return products;
    }
    [HttpGet]
    [Route("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<Product>> GetById(int id, [FromServices] DataContext context)
    {
        var product = await context.Products.Include(x => x.Category).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (product.Id != id || product.Id == null)
            return NotFound(new { message = "Produto não encontrado" });

        return Ok(product);
    }
    [HttpGet]
    [Route("categories/{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<Product>>> GetByCategory([FromServices] DataContext context, int id)
    {
        var products = await context.Products.Include(x => x.Category).AsNoTracking().Where(x => x.CategoryId == id).ToListAsync();
        return Ok(products);
    }
    [HttpPost]
    [Route("")]
    [Authorize(Roles = "employee")]
    public async Task<ActionResult<Product>> Post(
        [FromServices] DataContext context,
        [FromBody] Product model
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            context.Products.Add(model);
            await context.SaveChangesAsync();
            return Ok(model);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Não foi possível adicionar produto", ex.Message });
        }
    }
    [HttpPut]
    [Route("{id:int}")]
    [Authorize(Roles = "employee")]
    public async Task<ActionResult<Product>> Put(
        int id,
        [FromServices] DataContext context,
        [FromBody] Product model
    )
    {
        if (model.Id != id)
            return NotFound(new { message = "Produto não encontrado" });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            context.Entry<Product>(model).State = EntityState.Modified;
            context.SaveChangesAsync();
            return Ok(model);
        }
        catch (DbUpdateConcurrencyException)
        {
            return BadRequest(new { message = "Produto já foi atualizado" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Não foi possível atualizar produto", ex.Message });
        }
    }
    [HttpDelete]
    [Route("{id:int}")]
    [Authorize(Roles = "employee")]
    public async Task<ActionResult<Product>> Delete(
        int id,
        [FromServices] DataContext context)
    {
        var product = await context.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (product == null)
            return NotFound(new { message = "Produto não encontrado" });

        try
        {
            context.Products.Remove(product);
            await context.SaveChangesAsync();
            return Ok(new { message = "Produto excluído com sucesso!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Não foi possível remover produto", ex.Message });
        }
    }
}