<script>
    document.getElementById("editProfileForm").addEventListener("submit", function(event) {
        var newPassword = document.getElementById("newPassword").value;
    var confirmNewPassword = document.getElementById("confirmNewPassword").value;
    if (newPassword !== confirmNewPassword) {
        document.getElementById("error-message").innerText = "Новий пароль і підтвердження пароля не співпадають.";
    event.preventDefault();
        }
    });
</script>