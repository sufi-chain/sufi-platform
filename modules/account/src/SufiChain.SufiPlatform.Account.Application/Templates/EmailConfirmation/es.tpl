<div style="color:#111827;">
    <p style="margin:0 0 16px; font-size:16px; line-height:28px;">Hola{{ if model.userName }} {{ model.userName }}{{ end }},</p>
    <p style="margin:0 0 16px; font-size:15px; line-height:26px; color:#374151;">Confirme su dirección de correo haciendo clic en el botón siguiente.</p>
    <div style="margin:0 0 24px;">
        <a href="{{ model.link }}" style="display:inline-block; padding:12px 22px; background-color:#111827; color:#ffffff; text-decoration:none; border-radius:12px; font-size:14px; font-weight:700;">Confirmar correo</a>
    </div>
    <p style="margin:0; font-size:14px; line-height:24px; color:#6b7280;">Si no creó una cuenta, puede ignorar este mensaje.</p>
</div>
