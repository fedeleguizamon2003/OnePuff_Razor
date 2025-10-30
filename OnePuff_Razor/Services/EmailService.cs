using System.Net;
using System.Net.Mail;
using System.Text;
using OnePuff_Razor.Models;

namespace OnePuff_Razor.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        public EmailService(IConfiguration config) => _config = config;

        /// <summary>
        /// Envía el "ticket" del pedido como HTML al email del cliente.
        /// </summary>
        public async Task EnviarTicketAsync(string emailDestino, Pedido pedido)
        {
            var smtp = _config.GetSection("Smtp");

            using var mensaje = new MailMessage(smtp["From"], emailDestino)
            {
                Subject = $"Factura / Ticket de tu compra - Pedido #{pedido.PedidoId}",
                Body = ConstruirHtmlTicket(pedido),
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            using var cliente = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"] ?? "587"))
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtp["User"], smtp["Pass"])
            };

            await cliente.SendMailAsync(mensaje);
        }

        /// <summary>
        /// Alternativa simple si querés pasar un cuerpo ya armado (texto/HTML).
        /// </summary>
        public async Task EnviarFacturaAsync(string emailDestino, string cuerpo, bool esHtml = false)
        {
            var smtp = _config.GetSection("Smtp");

            using var mensaje = new MailMessage(smtp["From"], emailDestino)
            {
                Subject = "Factura de tu compra - OnePuff",
                Body = cuerpo,
                IsBodyHtml = esHtml,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            using var cliente = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"] ?? "587"))
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtp["User"], smtp["Pass"])
            };

            await cliente.SendMailAsync(mensaje);
        }

        private string ConstruirHtmlTicket(Pedido p)
        {
            var filas = string.Join("", p.Detalles.Select(d =>
                $"<tr>" +
                $"<td>{d.Producto?.Nombre ?? $"Producto #{d.ProductoId}"}</td>" +
                $"<td style='text-align:center'>{d.Cantidad}</td>" +
                $"<td style='text-align:right'>$ {d.PrecioUnitario:N2}</td>" +
                $"<td style='text-align:right'>$ {(d.PrecioUnitario * d.Cantidad):N2}</td>" +
                $"</tr>"
            ));

            return $@"
<h3 style='margin:0 0 10px 0;font-family:Arial'>Gracias por tu compra</h3>
<p style='font-family:Arial;margin:0 0 4px 0'><b>Pedido:</b> #{p.PedidoId} - {p.FechaPedido:g}</p>
<p style='font-family:Arial;margin:0 0 12px 0'><b>Entrega:</b> {p.DireccionEntrega}</p>

<table border='1' cellpadding='6' cellspacing='0' style='border-collapse:collapse;width:100%;font-family:Arial;font-size:14px'>
  <thead style='background:#f5f5f5'>
    <tr>
      <th align='left'>Producto</th>
      <th>Cant</th>
      <th align='right'>Precio</th>
      <th align='right'>Subtotal</th>
    </tr>
  </thead>
  <tbody>
    {filas}
  </tbody>
  <tfoot>
    <tr>
      <td colspan='3' style='text-align:right'><b>Total</b></td>
      <td style='text-align:right'><b>$ {p.Total:N2}</b></td>
    </tr>
  </tfoot>
</table>

<p style='font-family:Arial;margin-top:12px;color:#666'>OnePuff • ¡Gracias por elegirnos!</p>";
        }
    }
}
