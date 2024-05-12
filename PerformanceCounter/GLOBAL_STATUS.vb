
''' <summary>
''' ``SHOW GLOBAL STATUS;``
''' </summary>
Public Class GLOBAL_STATUS

    Public Property Aborted_clients
    Public Property Aborted_connects
    Public Property Acl_cache_items_count
    Public Property Binlog_cache_disk_use
    Public Property Binlog_cache_use
    Public Property Binlog_stmt_cache_disk_use
    Public Property Binlog_stmt_cache_use
    Public Property Bytes_received
    Public Property Bytes_sent
    Public Property Caching_sha2_password_rsa_public_key
    Public Property Com_admin_commands
    Public Property Com_assign_to_keycache
    Public Property Com_alter_db
    Public Property Com_alter_event
    Public Property Com_alter_function
    Public Property Com_alter_instance
    Public Property Com_alter_procedure
    Public Property Com_alter_resource_group
    Public Property Com_alter_server
    Public Property Com_alter_table
    Public Property Com_alter_tablespace
    Public Property Com_alter_user
    Public Property Com_alter_user_default_role
    Public Property Com_analyze
    Public Property Com_begin
    Public Property Com_binlog
    Public Property Com_call_procedure
    Public Property Com_change_db
    Public Property Com_change_master
    Public Property Com_change_repl_filter
    Public Property Com_change_replication_source
    Public Property Com_check
    Public Property Com_checksum
    Public Property Com_clone
    Public Property Com_commit
    Public Property Com_create_db
    Public Property Com_create_event
    Public Property Com_create_function
    Public Property Com_create_index
    Public Property Com_create_procedure
    Public Property Com_create_role
    Public Property Com_create_server
    Public Property Com_create_table
    Public Property Com_create_resource_group
    Public Property Com_create_trigger
    Public Property Com_create_udf
    Public Property Com_create_user
    Public Property Com_create_view
    Public Property Com_create_spatial_reference_system
    Public Property Com_dealloc_sql
    Public Property Com_delete
    Public Property Com_delete_multi
    Public Property Com_do
Com_drop_db
Com_drop_event
Com_drop_function
Com_drop_index
Com_drop_procedure
Com_drop_resource_group
Com_drop_role
Com_drop_server
Com_drop_spatial_reference_system
Com_drop_table
Com_drop_trigger
Com_drop_user
Com_drop_view
Com_empty_query
Com_execute_sql
Com_explain_other
Com_flush
Com_get_diagnostics
Com_grant
Com_grant_roles
Com_ha_close
Com_ha_open
Com_ha_read
Com_help
Com_import
Com_insert
Com_insert_select
Com_install_component
Com_install_plugin
Com_kill
Com_load
Com_lock_instance
Com_lock_tables
Com_optimize
Com_preload_keys
Com_prepare_sql
Com_purge
Com_purge_before_date
Com_release_savepoint
Com_rename_table
Com_rename_user
Com_repair
Com_replace
Com_replace_select
Com_reset
Com_resignal
Com_restart
Com_revoke
Com_revoke_all
Com_revoke_roles
Com_rollback
Com_rollback_to_savepoint
Com_savepoint
Com_select
Com_set_option
Com_set_password
Com_set_resource_group
Com_set_role
Com_signal
Com_show_binlog_events
Com_show_binlogs
Com_show_charsets
Com_show_collations
    Public Property Com_show_create_db
    Public Property Com_show_create_event
    Public Property Com_show_create_func
    Public Property Com_show_create_proc
    Public Property Com_show_create_table
    Public Property Com_show_create_trigger
    Public Property Com_show_databases
    Public Property Com_show_engine_logs
    Public Property Com_show_engine_mutex
    Public Property Com_show_engine_status
    Public Property Com_show_events
Com_show_errors
Com_show_fields
Com_show_function_code
Com_show_function_status
Com_show_grants
Com_show_keys
Com_show_master_status
Com_show_open_tables
Com_show_plugins
Com_show_privileges
Com_show_procedure_code
Com_show_procedure_status
Com_show_processlist
Com_show_profile
Com_show_profiles
Com_show_relaylog_events
Com_show_replicas
Com_show_slave_hosts
Com_show_replica_status
Com_show_slave_status
Com_show_status
Com_show_storage_engines
Com_show_table_status
Com_show_tables
Com_show_triggers
Com_show_variables
Com_show_warnings
Com_show_create_user
Com_shutdown
Com_replica_start
Com_slave_start
Com_replica_stop
Com_slave_stop
Com_group_replication_start
Com_group_replication_stop
Com_stmt_execute
Com_stmt_close
Com_stmt_fetch
Com_stmt_prepare
Com_stmt_reset
Com_stmt_send_long_data
Com_truncate
Com_uninstall_component
Com_uninstall_plugin
Com_unlock_instance
Com_unlock_tables
Com_update
Com_update_multi
Com_xa_commit
Com_xa_end
Com_xa_prepare
Com_xa_recover
Com_xa_rollback
Com_xa_start
Com_stmt_reprepare
Connection_errors_accept
Connection_errors_internal
Connection_errors_max_connections
Connection_errors_peer_address
Connection_errors_select
Connection_errors_tcpwrap
Connections
Created_tmp_disk_tables
Created_tmp_files
Created_tmp_tables
Current_tls_ca
Current_tls_capath
Current_tls_cert
Current_tls_cipher
Current_tls_ciphersuites
Current_tls_crl
Current_tls_crlpath
Current_tls_key
Current_tls_version
Delayed_errors
Delayed_insert_threads
Delayed_writes
Error_log_buffered_bytes
Error_log_buffered_events
Error_log_expired_events
Error_log_latest_write
Flush_commands
Global_connection_memory
Handler_commit
Handler_delete
Handler_discover
Handler_external_lock
Handler_mrr_init
Handler_prepare
Handler_read_first
Handler_read_key
Handler_read_last
Handler_read_next
Handler_read_prev
Handler_read_rnd
Handler_read_rnd_next
Handler_rollback
Handler_savepoint
Handler_savepoint_rollback
Handler_update
Handler_write
Innodb_buffer_pool_dump_status
Innodb_buffer_pool_load_status
Innodb_buffer_pool_resize_status
Innodb_buffer_pool_resize_status_code
Innodb_buffer_pool_resize_status_progress
Innodb_buffer_pool_pages_data
Innodb_buffer_pool_bytes_data
Innodb_buffer_pool_pages_dirty
Innodb_buffer_pool_bytes_dirty
Innodb_buffer_pool_pages_flushed
Innodb_buffer_pool_pages_free
Innodb_buffer_pool_pages_misc
Innodb_buffer_pool_pages_total
Innodb_buffer_pool_read_ahead_rnd
Innodb_buffer_pool_read_ahead
Innodb_buffer_pool_read_ahead_evicted
Innodb_buffer_pool_read_requests
Innodb_buffer_pool_reads
Innodb_buffer_pool_wait_free
Innodb_buffer_pool_write_requests
Innodb_data_fsyncs
Innodb_data_pending_fsyncs
Innodb_data_pending_reads
Innodb_data_pending_writes
Innodb_data_read
Innodb_data_reads
Innodb_data_writes
Innodb_data_written
Innodb_dblwr_pages_written
Innodb_dblwr_writes
Innodb_redo_log_read_only
Innodb_redo_log_uuid
Innodb_redo_log_checkpoint_lsn
Innodb_redo_log_current_lsn
Innodb_redo_log_flushed_to_disk_lsn
Innodb_redo_log_logical_size
Innodb_redo_log_physical_size
Innodb_redo_log_capacity_resized
Innodb_redo_log_resize_status
Innodb_log_waits
Innodb_log_write_requests
Innodb_log_writes
Innodb_os_log_fsyncs
Innodb_os_log_pending_fsyncs
Innodb_os_log_pending_writes
Innodb_os_log_written
Innodb_page_size
Innodb_pages_created
Innodb_pages_read
Innodb_pages_written
Innodb_redo_log_enabled
Innodb_row_lock_current_waits
Innodb_row_lock_time
Innodb_row_lock_time_avg
Innodb_row_lock_time_max
Innodb_row_lock_waits
Innodb_rows_deleted
Innodb_rows_inserted
Innodb_rows_read
Innodb_rows_updated
Innodb_system_rows_deleted
Innodb_system_rows_inserted
Innodb_system_rows_read
Innodb_system_rows_updated
Innodb_sampled_pages_read
Innodb_sampled_pages_skipped
Innodb_num_open_files
Innodb_truncated_status_writes
Innodb_undo_tablespaces_total
Innodb_undo_tablespaces_implicit
Innodb_undo_tablespaces_explicit
Innodb_undo_tablespaces_active
Key_blocks_not_flushed
Key_blocks_unused
Key_blocks_used
Key_read_requests
Key_reads
Key_write_requests
Key_writes
Locked_connects
Max_execution_time_exceeded
Max_execution_time_set
Max_execution_time_set_failed
Max_used_connections
Max_used_connections_time
Mysqlx_aborted_clients
Mysqlx_address
Mysqlx_bytes_received
Mysqlx_bytes_received_compressed_payload
Mysqlx_bytes_received_uncompressed_frame
Mysqlx_bytes_sent
Mysqlx_bytes_sent_compressed_payload
Mysqlx_bytes_sent_uncompressed_frame
Mysqlx_compression_algorithm
Mysqlx_compression_level
Mysqlx_connection_accept_errors
Mysqlx_connection_errors
Mysqlx_connections_accepted
Mysqlx_connections_closed
Mysqlx_connections_rejected
Mysqlx_crud_create_view
Mysqlx_crud_delete
Mysqlx_crud_drop_view
Mysqlx_crud_find
Mysqlx_crud_insert
Mysqlx_crud_modify_view
Mysqlx_crud_update
Mysqlx_cursor_close
Mysqlx_cursor_fetch
Mysqlx_cursor_open
Mysqlx_errors_sent
Mysqlx_errors_unknown_message_type
Mysqlx_expect_close
Mysqlx_expect_open
Mysqlx_init_error
Mysqlx_messages_sent
Mysqlx_notice_global_sent
Mysqlx_notice_other_sent
Mysqlx_notice_warning_sent
Mysqlx_notified_by_group_replication
Mysqlx_port
Mysqlx_prep_deallocate
Mysqlx_prep_execute
Mysqlx_prep_prepare
Mysqlx_rows_sent
Mysqlx_sessions
Mysqlx_sessions_accepted
Mysqlx_sessions_closed
Mysqlx_sessions_fatal_error
Mysqlx_sessions_killed
Mysqlx_sessions_rejected
Mysqlx_socket
Mysqlx_ssl_accepts
Mysqlx_ssl_active
Mysqlx_ssl_cipher
Mysqlx_ssl_cipher_list
Mysqlx_ssl_ctx_verify_depth
Mysqlx_ssl_ctx_verify_mode
Mysqlx_ssl_finished_accepts
Mysqlx_ssl_server_not_after
Mysqlx_ssl_server_not_before
Mysqlx_ssl_verify_depth
Mysqlx_ssl_verify_mode
Mysqlx_ssl_version
Mysqlx_stmt_create_collection
Mysqlx_stmt_create_collection_index
Mysqlx_stmt_disable_notices
Mysqlx_stmt_drop_collection
Mysqlx_stmt_drop_collection_index
Mysqlx_stmt_enable_notices
Mysqlx_stmt_ensure_collection
Mysqlx_stmt_execute_mysqlx
Mysqlx_stmt_execute_sql
Mysqlx_stmt_execute_xplugin
Mysqlx_stmt_get_collection_options
Mysqlx_stmt_kill_client
Mysqlx_stmt_list_clients
Mysqlx_stmt_list_notices
Mysqlx_stmt_list_objects
Mysqlx_stmt_modify_collection_options
Mysqlx_stmt_ping
Mysqlx_worker_threads
Mysqlx_worker_threads_active
Not_flushed_delayed_rows
Ongoing_anonymous_transaction_count
Open_files
Open_streams
Open_table_definitions
Open_tables
Opened_files
Opened_table_definitions
Opened_tables
Performance_schema_accounts_lost
Performance_schema_cond_classes_lost
Performance_schema_cond_instances_lost
Performance_schema_digest_lost
Performance_schema_file_classes_lost
Performance_schema_file_handles_lost
Performance_schema_file_instances_lost
Performance_schema_hosts_lost
Performance_schema_index_stat_lost
Performance_schema_locker_lost
Performance_schema_memory_classes_lost
Performance_schema_metadata_lock_lost
Performance_schema_mutex_classes_lost
Performance_schema_mutex_instances_lost
Performance_schema_nested_statement_lost
Performance_schema_prepared_statements_lost
Performance_schema_program_lost
Performance_schema_rwlock_classes_lost
Performance_schema_rwlock_instances_lost
Performance_schema_session_connect_attrs_longest_seen
Performance_schema_session_connect_attrs_lost
Performance_schema_socket_classes_lost
Performance_schema_socket_instances_lost
Performance_schema_stage_classes_lost
Performance_schema_statement_classes_lost
Performance_schema_table_handles_lost
Performance_schema_table_instances_lost
Performance_schema_table_lock_stat_lost
Performance_schema_thread_classes_lost
Performance_schema_thread_instances_lost
Performance_schema_users_lost
Prepared_stmt_count
Queries
Questions
Replica_open_temp_tables
Resource_group_supported
Rsa_public_key
Secondary_engine_execution_count
Select_full_join
Select_full_range_join
Select_range
Select_range_check
Select_scan
Slave_open_temp_tables
Slow_launch_threads
Slow_queries
Sort_merge_passes
Sort_range
Sort_rows
Sort_scan
Ssl_accept_renegotiates
Ssl_accepts
Ssl_callback_cache_hits
Ssl_cipher
Ssl_cipher_list
Ssl_client_connects
Ssl_connect_renegotiates
Ssl_ctx_verify_depth
Ssl_ctx_verify_mode
Ssl_default_timeout
Ssl_finished_accepts
Ssl_finished_connects
Ssl_server_not_after
Ssl_server_not_before
Ssl_session_cache_hits
Ssl_session_cache_misses
Ssl_session_cache_mode
Ssl_session_cache_overflows
Ssl_session_cache_size
Ssl_session_cache_timeout
Ssl_session_cache_timeouts
Ssl_sessions_reused
Ssl_used_session_cache_entries
Ssl_verify_depth
Ssl_verify_mode
Ssl_version
Table_locks_immediate
Table_locks_waited
Table_open_cache_hits
Table_open_cache_misses
Table_open_cache_overflows
Tc_log_max_pages_used
Tc_log_page_size
Tc_log_page_waits
Telemetry_traces_supported
Threads_cached
Threads_connected
Threads_created
Threads_running
Tls_library_version
Uptime
Uptime_since_flush_status


End Class
