<div style="font-family: Arial, sans-serif; line-height: 1.5;">
    <p>Hello{{ if model.userName }} {{ model.userName }}{{ end }},</p>
    <p>Your verification code is:</p>
    <p style="font-size: 24px; font-weight: bold; letter-spacing: 2px;">{{ model.code }}</p>
    <p>This code will expire soon. Do not share it with anyone.</p>
</div>
