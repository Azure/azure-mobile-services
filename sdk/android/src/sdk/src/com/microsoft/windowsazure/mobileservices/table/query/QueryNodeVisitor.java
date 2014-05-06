/*
Copyright (c) Microsoft Open Technologies, Inc.
All Rights Reserved
Apache 2.0 License
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
 
     http://www.apache.org/licenses/LICENSE-2.0
 
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 
See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
 */
package com.microsoft.windowsazure.mobileservices.table.query;

import com.microsoft.windowsazure.mobileservices.MobileServiceException;

/**
 * Interface of a query node visitor used to extend functionality.
 */
public interface QueryNodeVisitor<E> {

	/**
	 * Visit a constant node.
	 * 
	 * @param nodeIn
	 *            The node to visit
	 * @return Defined by the implementer.
	 * @throws MobileServiceException
	 */
	public E Visit(ConstantNode nodeIn) throws MobileServiceException;

	/**
	 * Visit a field node.
	 * 
	 * @param nodeIn
	 *            The node to visit
	 * @return Defined by the implementer.
	 * @throws MobileServiceException
	 */
	public E Visit(FieldNode nodeIn) throws MobileServiceException;

	/**
	 * Visit a unary operator node.
	 * 
	 * @param nodeIn
	 *            The node to visit
	 * @return Defined by the implementer.
	 * @throws MobileServiceException
	 */
	public E Visit(UnaryOperatorNode nodeIn) throws MobileServiceException;

	/**
	 * Visit a binary operator node.
	 * 
	 * @param nodeIn
	 *            The node to visit
	 * @return Defined by the implementer.
	 * @throws MobileServiceException
	 */
	public E Visit(BinaryOperatorNode nodeIn) throws MobileServiceException;

	/**
	 * Visit a function call node.
	 * 
	 * @param nodeIn
	 *            The node to visit
	 * @return Defined by the implementer.
	 * @throws MobileServiceException
	 */
	public E Visit(FunctionCallNode nodeIn) throws MobileServiceException;
}