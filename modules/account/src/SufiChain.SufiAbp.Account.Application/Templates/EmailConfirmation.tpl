<div style="font-family: Arial, sans-serif; line-height: 1.5;">
    <p>Hello{{ if model.userName }} {{ model.userName }}{{ end }},</p>
    <p>Please confirm your email address by clicking the link below:</p>
    <p><a href="{{ model.link }}">Confirm email</a></p>
    <p>If you did not create an account, you can ignore this message.</p>
</div>
