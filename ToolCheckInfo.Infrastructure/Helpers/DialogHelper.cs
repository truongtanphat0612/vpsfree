using System.Windows.Forms;

namespace ToolCheckInfo.Infrastructure.Helpers;

public class DialogHelper
{
	public static bool ShowDialogConfirm(string message)
	{
		DialogResult resDialog = MessageBox.Show(message, "Thông báo", MessageBoxButtons.OKCancel);
		return resDialog == DialogResult.OK;
	}
}
