<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>{{model.subject}}</title>
</head>
<body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
    <div style="max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;">
        <div style="background-color: #007bff; color: #fff; padding: 10px; text-align: center; border-radius: 5px 5px 0 0;">
            <h1>Informatique Alumni</h1>
        </div>
        <div style="padding: 20px;">
            <h3>Hello {{model.recipient_name}},</h3>
            <p>{{model.body_content}}</p>
        </div>
        <div style="margin-top: 20px; padding-top: 10px; border-top: 1px solid #eee; font-size: 0.9em; color: #777; text-align: center;">
            <p>&copy; 2026 Informatique Alumni Association. All rights reserved.</p>
        </div>
    </div>
</body>
</html>
