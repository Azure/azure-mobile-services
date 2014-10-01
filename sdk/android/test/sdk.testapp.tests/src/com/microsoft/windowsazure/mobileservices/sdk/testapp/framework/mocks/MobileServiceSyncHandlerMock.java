package com.microsoft.windowsazure.mobileservices.sdk.testapp.framework.mocks;

import com.google.common.base.Function;
import com.google.gson.JsonObject;
import com.microsoft.windowsazure.mobileservices.sdk.testapp.test.types.CustomFunctionTwoParameters;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.RemoteTableOperationProcessor;
import com.microsoft.windowsazure.mobileservices.table.sync.operations.TableOperation;
import com.microsoft.windowsazure.mobileservices.table.sync.push.MobileServicePushCompletionResult;
import com.microsoft.windowsazure.mobileservices.table.sync.synchandler.MobileServiceSyncHandlerException;
import com.microsoft.windowsazure.mobileservices.table.sync.synchandler.SimpleSyncHandler;

public class MobileServiceSyncHandlerMock extends SimpleSyncHandler {

	public CustomFunctionTwoParameters<RemoteTableOperationProcessor, TableOperation, JsonObject> tableOperationAction;
	public Function<MobileServicePushCompletionResult, Void> pushCompleteAction;

	public MobileServicePushCompletionResult PushCompletionResult;

	@Override
	public JsonObject executeTableOperation(RemoteTableOperationProcessor processor, TableOperation operation) throws MobileServiceSyncHandlerException {
		// TODO Auto-generated method stub

		if (tableOperationAction != null) {
			try {
				return tableOperationAction.apply(processor, operation);
			} catch (Exception e) {
				return null;
			}
		} else {
			return super.executeTableOperation(processor, operation);
		}

	}

	@Override
	public void onPushComplete(MobileServicePushCompletionResult pushCompletionResult) throws MobileServiceSyncHandlerException {

		this.PushCompletionResult = pushCompletionResult;

		if (this.pushCompleteAction != null) {
			pushCompleteAction.apply(pushCompletionResult);
		} else {
			super.onPushComplete(pushCompletionResult);
		}
	}
}
