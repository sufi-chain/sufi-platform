<div style="color:#111827;">
    <p style="margin:0 0 16px; font-size:16px; line-height:28px;">Hello{{ if model.userName }} {{ model.userName }}{{ end }},</p>
    <p style="margin:0 0 16px; font-size:15px; line-height:26px; color:#374151;">Use the verification code below to continue.</p>
    <div style="margin:0 0 24px; padding:18px 20px; background-color:#f9fafb; border:1px solid #e5e7eb; border-radius:16px; text-align:center;">
        <div style="font-size:30px; line-height:38px; font-weight:700; letter-spacing:6px; color:#111827;">{{ model.code }}</div>
    </div>
    <p style="margin:0; font-size:14px; line-height:24px; color:#6b7280;">This code will expire soon. Do not share it with anyone.</p>
</div>
