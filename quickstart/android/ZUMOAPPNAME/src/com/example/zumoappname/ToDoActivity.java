package com.example.zumoappname;

import static com.microsoft.windowsazure.mobileservices.MobileServiceTable.MobileServiceQueryOperations.val;

import java.net.MalformedURLException;
import java.util.List;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.ProgressDialog;
import android.os.Bundle;
import android.view.View;
import android.widget.EditText;
import android.widget.ListView;

import com.microsoft.windowsazure.mobileservices.MobileServiceClient;
import com.microsoft.windowsazure.mobileservices.MobileServiceTable;
import com.microsoft.windowsazure.mobileservices.ServiceFilterResponse;
import com.microsoft.windowsazure.mobileservices.TableOperationCallback;
import com.microsoft.windowsazure.mobileservices.TableQueryCallback;

public class ToDoActivity extends Activity {
	
	private MobileServiceClient mClient;
	private MobileServiceTable mToDoTable;
	private ToDoItemAdapter mAdapter;
	private EditText mTextNewToDo;
	private ProgressDialog mProgressDialog;

	/**
	 * Initializes the activity
	 */
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_to_do);

		// Initialize the progress dialog
		mProgressDialog = new ProgressDialog(this);
		mProgressDialog.setMessage("Loading...");
		mProgressDialog.setCancelable(false);

		try {
			// Create the Mobile Service Client instance, using the provided
			// Mobile Service URL and key
			mClient = new MobileServiceClient(
					"ZUMOAPPURL",
					"ZUMOAPPKEY"
					);

			// Get the Mobile Service Table instance to use
			mToDoTable = mClient.getTable("todoitem");

			mTextNewToDo = (EditText) findViewById(R.id.textNewToDo);

			// Create an adapter to bind the items with the view
			mAdapter = new ToDoItemAdapter(this, R.layout.row_list_to_do);
			ListView listViewToDo = (ListView) findViewById(R.id.listViewToDo);
			listViewToDo.setAdapter(mAdapter);

			mProgressDialog.show();

			// Load the items from the Mobile Service
			refreshItemsFromTable();

		} catch (MalformedURLException e) {
			createAndShowDialog(
					new Exception(
							"There was an error creating the Mobile Service. Verify the URL"),
					"Error");
		}
	}

	/**
	 * Mark an item as completed
	 * 
	 * @param item
	 *            The item to mark
	 */
	public void checkItem(ToDoItem item) {
		if (mClient == null) {
			return;
		}

		// Set the item as completed and update it in the table
		item.setComplete(true);

		mProgressDialog.show();
		mToDoTable.update(item, new TableOperationCallback<ToDoItem>() {

			public void onSuccess(ToDoItem entity) {
				if (entity.isComplete()) {
					mAdapter.remove(entity);
				}
				mProgressDialog.dismiss();
			}

			public void onError(Exception exception,
					ServiceFilterResponse response) {
				mProgressDialog.dismiss();
				createAndShowDialog(exception, "Error");
			}

		});
	}

	/**
	 * Add a new item
	 * 
	 * @param view
	 *            The view that originated the call
	 */
	public void addItem(View view) {
		if (mClient == null) {
			return;
		}

		// Create a new item
		ToDoItem item = new ToDoItem();

		item.setText(mTextNewToDo.getText().toString());
		item.setComplete(false);

		mProgressDialog.show();
		// Insert the new item
		mToDoTable.insert(item, new TableOperationCallback<ToDoItem>() {
			public void onSuccess(ToDoItem entity) {
				if (!entity.isComplete()) {
					mAdapter.add(entity);
					mProgressDialog.dismiss();
				}
			}

			public void onError(Exception exception,
					ServiceFilterResponse response) {
				mProgressDialog.dismiss();
				createAndShowDialog(exception, "Error");
			}
		});

		mTextNewToDo.setText("");
	}

	/**
	 * Refresh the list with the items in the Mobile Service Table
	 */
	private void refreshItemsFromTable() {

		// Get the items that weren't marked as completed and add them in the
		// adapter
		mToDoTable.where().field("complete").eq(val(false))
				.execute(ToDoItem.class, new TableQueryCallback<ToDoItem>() {
					public void onSuccess(List<ToDoItem> result, int count) {
						mAdapter.clear();

						for (ToDoItem item : result) {
							mAdapter.add(item);
						}

						mProgressDialog.dismiss();
					}

					public void onError(Exception exception,
							ServiceFilterResponse response) {
						mProgressDialog.dismiss();
						createAndShowDialog(exception, "Error");
					}
				});
	}

	/**
	 * Creates a dialog and shows it
	 * @param exception The exception to show in the dialog
	 * @param title The dialog title
	 */
	private void createAndShowDialog(Exception exception, String title) {
		createAndShowDialog(exception.toString(), title);
	}

	/**
	 * Creates a dialog and shows it
	 * 
	 * @param message
	 *            The dialog message
	 * @param title
	 *            The dialog title
	 */
	private void createAndShowDialog(String message, String title) {
		AlertDialog.Builder builder = new AlertDialog.Builder(this);

		builder.setMessage(message);
		builder.setTitle(title);
		builder.create().show();
	}
}
