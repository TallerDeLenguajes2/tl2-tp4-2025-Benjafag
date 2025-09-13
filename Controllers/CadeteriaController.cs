using System.Text.Json;
using Biblioteca;
using Microsoft.AspNetCore.Mvc;

public class AgregarPedidoDTO {
  public string Obs { get; set; }
  public EstadoPedido Estado { get; set; }
  public Cliente Cliente { get; set; }
}

[ApiController]
[Route("[controller]")]
public class CadeteriaController : ControllerBase {
  private Cadeteria _cadeteria;
  private void GuardarPedidos(List<Pedido> pedidos) {
    System.IO.File.WriteAllText("./Backup/pedidos.json", JsonSerializer.Serialize(pedidos));
  }
  private void GuardarCadetes(List<Cadete> cadetes) {
    System.IO.File.WriteAllText("./Backup/cadetes.json", JsonSerializer.Serialize(cadetes));
  }

  public CadeteriaController()
  {
    var datos = new AccesoADatosJSON();
    Cadeteria cadeteria = datos.CargarCadeteria("./Backup/cadeteria.json", "./Backup/cadetes.json", "./Backup/pedidos.json");
    _cadeteria = cadeteria;
  }

  [HttpGet("Pedidos")]
  public ActionResult<List<Pedido>> GetPedidos() => Ok(_cadeteria.ListadoPedidos);

  [HttpGet("Cadetes")]
  public ActionResult<List<Cadete>> GetCadetes() => Ok(_cadeteria.ListadoCadetes);

  [HttpPost("AgregarPedido")]
  public IActionResult AgregarPedido([FromForm] AgregarPedidoDTO pedido)
  {
    if (pedido == null || pedido.Cliente == null ||
        string.IsNullOrEmpty(pedido.Obs) ||
        string.IsNullOrEmpty(pedido.Cliente.Nombre) ||
        string.IsNullOrEmpty(pedido.Cliente.Direccion) ||
        string.IsNullOrEmpty(pedido.Cliente.DatosReferenciaDireccion) ||
        pedido.Cliente.Telefono == 0)
      return BadRequest("Faltan datos");

    _cadeteria.DarAltaPedido(pedido.Obs, pedido.Cliente.Nombre, pedido.Cliente.Direccion, pedido.Cliente.Telefono, pedido.Cliente.DatosReferenciaDireccion);
    GuardarPedidos(_cadeteria.ListadoPedidos);
    return Ok(_cadeteria.ListadoPedidos.Last());
  }

  [HttpPut("AsignarPedido")]
  public IActionResult AsignarPedido([FromForm] int nroPedido, [FromForm] int idCadete)
  {
    if (idCadete == 0 || nroPedido == 0)
      return BadRequest("Faltan datos");

    if (!_cadeteria.ListadoCadetes.Any(c => c.Id == idCadete))
      return NotFound($"No existe cadete con el id {idCadete}");

    Pedido pedido = _cadeteria.ListadoPedidos.Find(p => p.Nro == nroPedido);

    if (pedido == null)
      return NotFound($"No existe pedido con el nro {nroPedido}");

    if (pedido.CadeteAsociado != null)
      return Conflict($"El pedido con el nro {nroPedido} ya se encuentra asignado");

    _cadeteria.AsignarPedido(nroPedido, idCadete);

    GuardarPedidos(_cadeteria.ListadoPedidos);
    GuardarCadetes(_cadeteria.ListadoCadetes);

    return NoContent();
  }

  [HttpPut("CambiarEstadoPedido")]
  public IActionResult CambiarEstadoPedido([FromForm] int nroPedido)
  {
    if (nroPedido == 0)
      return BadRequest("Faltan datos");

    Pedido pedido = _cadeteria.ListadoPedidos.Find(p => p.Nro == nroPedido);

    if (pedido == null)
      return NotFound($"No existe pedido con el nro {nroPedido}");

    if (pedido.Estado == EstadoPedido.Realizado)
      return Conflict($"El pedido {nroPedido} ya se encuentra realizado");

    if (pedido.CadeteAsociado == null)
      return Conflict($"El pedido {nroPedido} no se encuentra asignado a ningun cadete");

    _cadeteria.PedidoCompletado(pedido.Nro);

    GuardarPedidos(_cadeteria.ListadoPedidos);
    GuardarCadetes(_cadeteria.ListadoCadetes);

    return NoContent();
  }

  [HttpPut("CambiarCadetePedido")]
  public IActionResult CambiarCadetePedido([FromForm] int idCadete, [FromForm] int nroPedido)
  {
    if (idCadete == 0 || nroPedido == 0)
      return BadRequest("Faltan datos");

    if (!_cadeteria.ListadoCadetes.Any(c => c.Id == idCadete))
      return NotFound($"No existe cadete con el id {idCadete}");

    Pedido pedido = _cadeteria.ListadoPedidos.Find(p => p.Nro == nroPedido);

    if (pedido == null)
      return NotFound($"No existe pedido con el nro {nroPedido}");

    if (pedido.Estado == EstadoPedido.Realizado)
      return Conflict($"El pedido {pedido.Nro} ya se encuentra realizado");

    _cadeteria.ReasignarPedido(pedido.Nro, idCadete);

    GuardarPedidos(_cadeteria.ListadoPedidos);
    GuardarCadetes(_cadeteria.ListadoCadetes);

    return NoContent();
  }
}