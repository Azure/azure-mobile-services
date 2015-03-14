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

/**
 * QueryNodeODataWriter.java
 */
package com.microsoft.windowsazure.mobileservices.table.query;

import com.microsoft.windowsazure.mobileservices.table.DateTimeOffset;
import com.microsoft.windowsazure.mobileservices.table.serialization.DateSerializer;

import java.io.UnsupportedEncodingException;
import java.util.Date;
import java.util.Locale;

/**
 * Query node visitor used to generate OData filter.
 */
class QueryNodeODataWriter implements QueryNodeVisitor<QueryNode> {
    private StringBuilder mBuilder;

    /**
     * Constructor for QueryNodeODataWriter
     */
    QueryNodeODataWriter() {
        this.mBuilder = new StringBuilder();
    }

    private static String process(String s) {
        return "'" + percentEncode(sanitize(s)) + "'";
    }

    private static String process(Date date) {
        return "datetime'" + DateSerializer.serialize(date) + "'";
    }

    private static String process(DateTimeOffset dateTimeOffset) {
        return "datetimeoffset'" + DateSerializer.serialize(dateTimeOffset) + "'";
    }

    /**
     * Sanitizes the string to use in a oData query
     *
     * @param s The string to sanitize
     * @return The sanitized string
     */
    private static String sanitize(String s) {
        if (s != null) {
            return s.replace("'", "''");
        } else {
            return null;
        }
    }

    static String percentEncode(String s) {
        return percentEncode(s, "");
    }

    static String percentEncode(String s, String reserved) {
        if (s == null) {
            return null;
        }

        StringBuilder builder = new StringBuilder(s.length());

        int escapeStart = -1;

        for (int i = 0; i < s.length(); i++) {
            char c = s.charAt(i);

            if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || "-._~".indexOf(c) != -1 || reserved.indexOf(c) != -1) {
                if (escapeStart != -1) {
                    appendHex(builder, s.substring(escapeStart, i));
                    escapeStart = -1;
                }

                builder.append(c);
            } else if (escapeStart == -1) {
                escapeStart = i;
            }
        }

        if (escapeStart != -1) {
            appendHex(builder, s.substring(escapeStart, s.length()));
        }

        return builder.toString();
    }

    private static void appendHex(StringBuilder builder, String s) {
        try {
            for (byte b : s.getBytes("UTF-8")) {
                appendHex(builder, b);
            }
        } catch (UnsupportedEncodingException e) {
            // UTF-8 should support any string
        }
    }

    private static void appendHex(StringBuilder sb, byte b) {
        sb.append('%');
        sb.append(String.format("%02X", b));
    }

    /**
     * Gets the StringBuilder with the OData representation of the node
     */
    StringBuilder getBuilder() {
        return this.mBuilder;
    }

    @Override
    public QueryNode visit(ConstantNode node) {
        Object value = node.getValue();
        String constant = value != null ? value.toString() : "null";

        if (value instanceof String) {
            constant = process((String) value);
        } else if (value instanceof DateTimeOffset) {
            constant = process((DateTimeOffset) value);
        }else if (value instanceof Date) {
            constant = process((Date) value);
        }

        this.mBuilder.append(constant);

        return node;
    }

    @Override
    public QueryNode visit(FieldNode node) {
        this.mBuilder.append(percentEncode(node.getFieldName(), "!$&'()*+,;=:@")); // odataIdentifier
        return node;
    }

    @Override
    public QueryNode visit(UnaryOperatorNode node) {
        if (node.getUnaryOperatorKind() == UnaryOperatorKind.Parenthesis) {
            this.mBuilder.append("(");

            if (node.getArgument() != null) {
                node.getArgument().accept(this);
            }

            this.mBuilder.append(")");
        } else {
            this.mBuilder.append(node.getUnaryOperatorKind().name().toLowerCase(Locale.getDefault()));

            if (node.getArgument() != null) {
                this.mBuilder.append("%20");
                node.getArgument().accept(this);
            }
        }

        return node;
    }

    @Override
    public QueryNode visit(BinaryOperatorNode node) {
        if (node.getLeftArgument() != null) {
            node.getLeftArgument().accept(this);
            this.mBuilder.append("%20");
        }

        this.mBuilder.append(node.getBinaryOperatorKind().name().toLowerCase(Locale.getDefault()));

        if (node.getRightArgument() != null) {
            this.mBuilder.append("%20");
            node.getRightArgument().accept(this);
        }

        return node;
    }

    @Override
    public QueryNode visit(FunctionCallNode node) {
        this.mBuilder.append(node.getFunctionCallKind().name().toLowerCase(Locale.getDefault()));
        this.mBuilder.append("(");

        boolean first = true;

        for (QueryNode argument : node.getArguments()) {
            if (!first) {
                this.mBuilder.append(",");
            } else {
                first = false;
            }

            argument.accept(this);
        }

        this.mBuilder.append(")");

        return node;
    }
}
